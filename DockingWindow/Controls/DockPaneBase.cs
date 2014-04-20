using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SunokoLibrary.Windows.Controls
{
    public class DockPaneBase : DockNode
    {
        public DockPaneBase()
        {
            _layout = new DockPaneLayoutEngine(this);
            _layout.PropertyChanged += new PropertyChangedEventHandler(_layout_PropertyChanged);
            SplitterVisible = _layout.SplitterVisible;
        }
        static DockPaneBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DockPaneBase), new FrameworkPropertyMetadata(typeof(DockPaneBase)));
        }
        DockPaneLayoutEngine _layout;
        System.Windows.Controls.Primitives.Thumb _splitter;

        public DockDirection Align
        {
            get { return (DockDirection)GetValue(AlignProperty); }
            set { SetValue(AlignProperty, value); }
        }
        public DockLength Length
        {
            get { return (DockLength)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }
        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public bool SplitterVisible
        {
            get { return (bool)GetValue(SplitterVisibleProperty); }
            set { SetValue(SplitterVisiblePropertyKey, value); }
        }
        public double ContentLeft
        {
            get { return (double)GetValue(ContentLeftProperty); }
            private set { SetValue(ContentLeftProperty, value); }
        }
        public double ContentTop
        {
            get { return (double)GetValue(ContentTopProperty); }
            private set { SetValue(ContentTopProperty, value); }
        }
        public double ContentWidth
        {
            get { return (double)GetValue(ContentWidthProperty); }
            private set { SetValue(ContentWidthProperty, value); }
        }
        public double ContentHeight
        {
            get { return (double)GetValue(ContentHeightProperty); }
            private set { SetValue(ContentHeightProperty, value); }
        }
        protected override DockNodeLayoutEngine LayoutEngine
        { get { return _layout; } }
        internal override Size ItemsPanelSize
        {
            get
            {
                var sz = new Size(
                    Math.Max(0, ActualWidth - ((Align == DockDirection.Left || Align == DockDirection.Right)
                    && SplitterVisible ? DockBayBase.GetSplitterWidth(this) : 0)),
                    Math.Max(0, ActualHeight - ((Align == DockDirection.Top || Align == DockDirection.Bottom)
                    && SplitterVisible ? DockBayBase.GetSplitterWidth(this) : 0)));
                return sz;
            }
        }

        public void Remove()
        {
            if (_layout.Parent == null)
                throw new InvalidOperationException("親要素が存在しない状態で自身を削除することはできません。");
            _layout.Parent.Target.Items.Remove(this);
        }
        public void MoveSplitter(DockDirection align, double distance)
        {
            var pane = (DockPaneLayoutEngine)LayoutEngine;
            DockPaneLayoutEngine splitPane;
            switch (align)
            {
                case DockDirection.Top:
                    splitPane = pane.Top;
                    break;
                case DockDirection.Bottom:
                    splitPane = pane.Bottom;
                    break;
                case DockDirection.Left:
                    splitPane = pane.Left;
                    break;
                case DockDirection.Right:
                    splitPane = pane.Right;
                    break;
                default:
                    throw new ArgumentException(
                        "引数alignはDockDirection.Top, Bottom, Left, Right以外入れることはできません。");
            }
            MoveSplitter(align, distance, splitPane);
        }
        internal override void InternalApplyConfig(DockBaseConfig config)
        {
            foreach (var c in config.Children)
            {
                var p = (DockPaneBase)c.Target.Target;
                p.Align = c.Align;
                p.Length = c.Size;
                p.Name = c.Name;
                Items.Add(p);
            }
        }
        internal override void InternalInitConfig(DockBaseConfig config)
        {
            var conf = (DockPaneBaseConfig)config;
            conf.Name = Name;
        }
        protected void MoveSplitter(DockDirection align, double distance, DockPaneLayoutEngine splitPane)
        {
            var pane = (DockPaneLayoutEngine)LayoutEngine;
            //isFamilyがtrueの時はouterに自Nodeがある
            //Bayへはまだ未対応Parent.Childrenを弄る必要がある
            var isFamily = splitPane.OwnNodes.Contains(pane) ? 1 : -1;
            //splitPaneの子要素Sizeの再計算を行うために親要素での自身が展開可能な最大サイズを出す。
            var splitPaneParent = (DockNode)splitPane.Parent.Target;
            var baseSz = (align == DockDirection.Top || align == DockDirection.Bottom
                ? splitPaneParent.ItemsPanelSize.Height : splitPaneParent.ItemsPanelSize.Width)
                - splitPane.Parent.PaneVisualTree
                .TakeWhile(l => l != splitPane)
                .Where(l => l.Align == align || l.Align == ~align)
                .Select(l => l.Align == DockDirection.Top || l.Align == DockDirection.Bottom
                    ? l.Target.ActualHeight : l.Target.ActualWidth)
                .Sum();
            var outer = (DockPaneBase)splitPane.Target;
            var index = Array.IndexOf(splitPane.Parent.PaneVisualTree.ToArray(), splitPane);
            var inners = splitPane.Parent.PaneVisualTree
                .Skip(index + 1)
                .Take(splitPane.Parent.PaneVisualTree
                    .Skip(index + 1)
                    .TakeWhile(l => l.Align != ~align)
                    .Count() + 1);

            //CheckResizableSize
            distance = ResizableDistance(align, isFamily * distance);
            //Resize
            outer.ResizeNodes(align, distance * isFamily, baseSz);
            baseSz -= (align == DockDirection.Top || align == DockDirection.Bottom
                ? outer.ActualHeight : outer.ActualWidth) + distance;
            foreach (var item in inners)
            {
                //alignはResizePaneのResize方向が入ってる。よって、alignは仕組み上、innersに
                //とってはResizePaneの存在する反対方向を指し示す値が入る。そのため、innersの
                //ResizePaneと接しておらず、Resize不要の反対alignPaneは"item.Align == align"
                //がtrueになる。これを利用してResize量を出す
                var childDistance = item.Align == align ? 0 : distance;
                ((DockPaneBase)item.Target).ResizeNodes(~align, childDistance * -isFamily, baseSz);
                //itemのResize分だけ子要素の展開可能Sizeを更新
                if (item.Align == align || item.Align == ~align)
                    baseSz -= (align == DockDirection.Top || align == DockDirection.Bottom
                        ? item.Target.ActualHeight : item.Target.ActualWidth) + childDistance;
            }
        }
        /// <summary>Splitterを持つDockPaneBaseにおいて、拡縮可能なサイズを計算します。</summary>
        /// <param name="align">拡縮する方向</param>
        /// <param name="distance">真の値の時は拡大。負の値の時は縮小です。</param>
        /// <returns></returns>
        double ResizableDistance(DockDirection align, double distance)
        {
            double res;
            var pane = (DockPaneLayoutEngine)LayoutEngine;
            DockPaneLayoutEngine splitPane;
            switch (align)
            {
                case DockDirection.Top:
                case DockDirection.Bottom:
                    splitPane = align == DockDirection.Top ? pane.Top : pane.Bottom;

                    if (distance > 0)
                        res = splitPane.Parent.GetChildrenOf(~align,
                            splitPane.Parent.PaneVisualTree.TakeWhile(l => l != splitPane).Count() + 1)
                            .Min(l => ((DockPaneBase)l.Target).ContentHeight);
                    else
                        res = -splitPane.GetChildrenOf(align)
                            .Min(l => ((DockPaneBase)l.Target).ContentHeight);
                    break;
                case DockDirection.Left:
                case DockDirection.Right:
                    splitPane = align == DockDirection.Left ? pane.Left : pane.Right;

                    if (distance > 0)
                        res = splitPane.Parent.GetChildrenOf(~align,
                            splitPane.Parent.PaneVisualTree.TakeWhile(l => l != splitPane).Count() + 1)
                            .Min(l => ((DockPaneBase)l.Target).ContentWidth);
                    else
                        res = -splitPane.GetChildrenOf(align)
                            .Min(l => ((DockPaneBase)l.Target).ContentWidth);
                    break;
                default:
                    throw new ArgumentException(
                        "引数AlignをDockDirection.Noneにすることはできません。");
            }
            return Math.Abs(distance) < Math.Abs(res) ? distance : res;
        }
        /// <summary>
        /// MoveSplitter用。Gridリサイズ時に内部のGridの分割サイズを
        /// 調整することによって内部がそのまま縮小される現象を回避します
        /// </summary>
        /// <param name="align">引数distance分だけサイズを増やしていくGridの方向</param>
        void ResizeNodes(DockDirection align, double distance, double parentSize)
        {
            if (align == DockDirection.None)
                throw new ArgumentException("引数alignをDockDirection.Noneにする事はできません。");

            var pane = (DockPaneLayoutEngine)LayoutEngine;
            var paneNode = this;
            var isFillNode =
                ((DockPaneLayoutEngine)LayoutEngine).Parent is DockBayLayoutEngine
                && ((DockPaneLayoutEngine)LayoutEngine).Parent.Children.First() == LayoutEngine;
            if ((Align == align || Align == ~align) && !isFillNode)
            {
                var actualSz = paneNode.Length.GetSize(parentSize);
                switch (Align)
                {
                    case DockDirection.Top:
                    case DockDirection.Bottom:
                        paneNode.Length = new DockLength(
                            (paneNode.ActualHeight + distance) / parentSize,
                            paneNode.Length.Pixel == double.PositiveInfinity
                                ? double.PositiveInfinity : paneNode.ActualHeight + distance);
                        break;
                    case DockDirection.Left:
                    case DockDirection.Right:
                        paneNode.Length = new DockLength(
                            (paneNode.ActualWidth + distance) / parentSize,
                            paneNode.Length.Pixel == double.PositiveInfinity
                                ? double.PositiveInfinity : paneNode.ActualWidth + distance);
                        break;
                }
            }
            var baseSz = (align == DockDirection.Top || align == DockDirection.Bottom
                ? ItemsPanelSize.Height : ItemsPanelSize.Width) + distance;

            foreach (var child in pane.Children)
            {
                var childNode = (DockPaneBase)child.Target;
                var childDistance = child.Align == ~align ? 0 : distance;
                childNode.ResizeNodes(align, childDistance, baseSz);
                if (child.Align == align || child.Align == ~align)
                    //itemのResize分だけ子要素の展開可能Sizeを更新
                    baseSz -= (align == DockDirection.Top || align == DockDirection.Bottom ?
                        childNode.ActualHeight : childNode.ActualWidth) + childDistance;

                //リサイズ方向と同AlignなPaneが見つかったら、以後のPaneの内部
                //サイズが変化することは無いため。よってループは終了する。
                if (child.Align == align)
                    break;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var splitter = (System.Windows.Controls.Primitives.Thumb)Template.FindName("DPB_Splitter", this);
            splitter.DragDelta += _splitter_DragDelta;
            _splitter = splitter;
        }
        void _layout_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SplitterVisible":
                    SplitterVisible = ((DockPaneLayoutEngine)LayoutEngine).SplitterVisible;
                    break;
                case "Align":
                    Align = _layout.Align;
                    break;
                case "Length":
                    Length = _layout.Length;
                    break;
            }
        }
        void _splitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            switch (Align)
            {
                case DockDirection.Top:
                    MoveSplitter(~Align, e.VerticalChange, (DockPaneLayoutEngine)LayoutEngine);
                    break;
                case DockDirection.Bottom:
                    MoveSplitter(~Align, -e.VerticalChange, (DockPaneLayoutEngine)LayoutEngine);
                    break;
                case DockDirection.Left:
                    MoveSplitter(~Align, e.HorizontalChange, (DockPaneLayoutEngine)LayoutEngine);
                    break;
                case DockDirection.Right:
                    MoveSplitter(~Align, -e.HorizontalChange, (DockPaneLayoutEngine)LayoutEngine);
                    break;
            }
            e.Handled = true;
        }

        public static readonly DependencyProperty AlignProperty = DependencyProperty.Register(
            "Align", typeof(DockDirection), typeof(DockPaneBase), new UIPropertyMetadata(DockDirection.None));
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
            "Length", typeof(DockLength), typeof(DockPaneBase), new FrameworkPropertyMetadata(new DockLength(), FrameworkPropertyMetadataOptions.AffectsParentMeasure, (sender, e) =>
                {
                    var obj = (DockPaneBase)sender;
                    var lay = (DockPaneLayoutEngine)obj.LayoutEngine;
                    lay.Length = (DockLength)e.NewValue;
                }));
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(FrameworkElement), typeof(DockPaneBase), new UIPropertyMetadata(null));
        public static readonly DependencyPropertyKey SplitterVisiblePropertyKey = DependencyProperty.RegisterReadOnly(
            "SplitterVisible", typeof(bool), typeof(DockPaneBase), new UIPropertyMetadata(true));
        public static readonly DependencyProperty ContentLeftProperty = DependencyProperty.Register(
            "ContentLeft", typeof(double), typeof(DockPaneBase), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty ContentTopProperty = DependencyProperty.Register(
            "ContentTop", typeof(double), typeof(DockPaneBase), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(
            "ContentWidth", typeof(double), typeof(DockPaneBase), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(
            "ContentHeight", typeof(double), typeof(DockPaneBase), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty SplitterVisibleProperty = SplitterVisiblePropertyKey.DependencyProperty;
    }

    [Serializable]
    public class DockPaneBaseConfig : DockBaseConfig
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public DockLength Size { get; set; }
        public DockDirection Align { get; set; }
        public DockPaneLayoutEngine Target { get; set; }
    }
}
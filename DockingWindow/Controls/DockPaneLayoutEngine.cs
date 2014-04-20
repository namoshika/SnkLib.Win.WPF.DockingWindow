using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SunokoLibrary.Windows.Controls
{
    public class DockPaneLayoutEngine : DockNodeLayoutEngine, INotifyPropertyChanged
    {
        public DockPaneLayoutEngine(DockPaneBase target)
            : base(target)
        {
            _ownNodes.Add(this);
            Align = DockDirection.None;
            SplitterVisible = true;
        }
        bool _paneVisible;
        bool _splitterVisible;
        DockDirection _align;
        VisibleMode _visibleMode;
        DockLength _length;

        public bool PaneVisible
        {
            get { return _paneVisible; }
            set
            {
                if (_paneVisible == value)
                    return;
                _paneVisible = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PaneVisible"));
            }
        }
        public bool SplitterVisible
        {
            get { return _splitterVisible; }
            set
            {
                if (_splitterVisible == value)
                    return;
                _splitterVisible = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SplitterVisible"));
            }
        }
        public DockLength Length
        {
            get { return _length; }
            set
            {
                if (_length == value)
                    return;

                _length = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Length"));
            }
        }
        public DockDirection Align
        {
            get { return _align; }
            private set
            {
                if (_align == value)
                    return;

                _align = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Align"));
            }
        }
        public VisibleMode VisibleMode
        {
            get { return _visibleMode; }
            set
            {
                if (_visibleMode == value)
                    return;
                _visibleMode = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VisibleMode"));
            }
        }
        public DockBayLayoutEngine Owner { get; private set; }
        public DockNodeLayoutEngine Parent { get; internal set; }
        public DockPaneLayoutEngine Top { get; internal set; }
        public DockPaneLayoutEngine Bottom { get; internal set; }
        public DockPaneLayoutEngine Left { get; internal set; }
        public DockPaneLayoutEngine Right { get; internal set; }
        public override IEnumerable<DockPaneLayoutEngine> PaneVisualTree { get { return Children; } }

        /// <summary>子ノード郡の間に新しいPaneを挿入します</summary>
        /// <param name="idx">後輩ノードになるPaneのインデックス</param>
        public override void Add(DockPaneLayoutEngine pane, DockDirection align, int idx)
        {
            if (pane == null)
                throw new ArgumentNullException("第1引数のneighをnullにする事はできません。");
            if (pane.Owner != null)
                throw new ArgumentException("第1引数のneighは使用中です。");
            if (align == DockDirection.None)
                throw new ArgumentException("第2引数のalignをDockDirection.Noneにすることはできません。");
            if (idx < 0 || idx > Children.Count)
                throw new ArgumentOutOfRangeException("第3引数のinsertIndexが有効範囲外です。");

            if (align != DockDirection.Top)
            {
                var btm = GetChildrenOf(DockDirection.Bottom, idx).FirstOrDefault();
                if (btm != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Bottom))
                        e.Bottom = btm.Bottom;
            }
            if (align != DockDirection.Bottom)
            {
                var top = GetChildrenOf(DockDirection.Top, idx).FirstOrDefault();
                if (top != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Top))
                        e.Top = top.Top;
            }
            if (align != DockDirection.Left)
            {
                var rgh = GetChildrenOf(DockDirection.Right, idx).FirstOrDefault();
                if (rgh != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Right))
                        e.Right = rgh.Right;
            }
            if (align != DockDirection.Right)
            {
                var lft = GetChildrenOf(DockDirection.Left, idx).FirstOrDefault();
                if (lft != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Left))
                        e.Left = lft.Left;
            }
            switch (align)
            {
                case DockDirection.Top:
                    foreach (var paneInner in pane.GetChildrenOf(~align))
                        paneInner.Bottom = pane;
                    foreach (var thisInner in GetChildrenOf(align, idx))
                        thisInner.Top = pane;
                    break;
                case DockDirection.Bottom:
                    foreach (var paneInner in pane.GetChildrenOf(~align))
                        paneInner.Top = pane;
                    foreach (var thisInner in GetChildrenOf(align, idx))
                        thisInner.Bottom = pane;
                    break;
                case DockDirection.Left:
                    foreach (var paneInner in pane.GetChildrenOf(~align))
                        paneInner.Right = pane;
                    foreach (var thisInner in GetChildrenOf(align, idx))
                        thisInner.Left = pane;
                    break;
                case DockDirection.Right:
                    foreach (var paneInner in pane.GetChildrenOf(~align))
                        paneInner.Left = pane;
                    foreach (var thisInner in GetChildrenOf(align, idx))
                        thisInner.Right = pane;
                    break;
            }
            pane.Initialize(this, Owner, align);

            //base.Addを初めに呼びだしてしまうと、Childrenが更新されるために
            //GetChildrenOfの挙動に以上が生じる。そのため、base.Addは最後に呼ぶ
            base.Add(pane, align, idx);
        }
        public override void Remove(DockPaneLayoutEngine pane)
        {
            if (!Children.Contains(pane))
                throw new ArgumentException("引数paneはこの要素の子要素ではありません。");

            var paneRmvAlign = pane.GetSucceedingDireWhenDeletingPane();

            #region Debug時用状態チェックコード
#if DEBUG
            if (pane.Align == DockDirection.None)
            {
                System.Diagnostics.Debug.Fail(
                    "StatusError", "自身の挿入方向が分かりません。");
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
            if (paneRmvAlign == DockDirection.None)
            {
                System.Diagnostics.Debug.Fail(
                    "StatusError", "自身を消去した後の領域を受け継ぐノードが存在しません。");
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
#endif
            #endregion

            if (pane.Children.Count > 0)
            {
                var newParent = pane.Children.Last();
                //newParent.Align = pane.Align;
                switch (pane.Align)
                {
                    case DockDirection.Top:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Bottom = newParent;
                        foreach (var p in GetChildrenOf(pane.Align, Children.IndexOf(pane) + 1))
                            p.Top = newParent;
                        break;
                    case DockDirection.Bottom:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Top = newParent;
                        foreach (var p in GetChildrenOf(pane.Align, Children.IndexOf(pane) + 1))
                            p.Bottom = newParent;
                        break;
                    case DockDirection.Left:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Right = newParent;
                        foreach (var p in GetChildrenOf(pane.Align, Children.IndexOf(pane) + 1))
                            p.Left = newParent;
                        break;
                    case DockDirection.Right:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Left = newParent;
                        foreach (var p in GetChildrenOf(pane.Align, Children.IndexOf(pane) + 1))
                            p.Right = newParent;
                        break;
                }
                //代替用子要素が持つ、自身と消去要素(paneの事)が接触している部分の
                //SplitPane情報を消去要素が同じ方向に持っているSplitPane情報に差し替え
                switch (paneRmvAlign)
                {
                    case DockDirection.Top:
                        newParent.Bottom = pane.Bottom;
                        break;
                    case DockDirection.Bottom:
                        newParent.Top = pane.Top;
                        break;
                    case DockDirection.Left:
                        newParent.Right = pane.Right;
                        break;
                    case DockDirection.Right:
                        newParent.Left = pane.Left;
                        break;
                }
            }
            else
            {
                //最初要素にも新親要素にもならない独身要素が消える際には削除方向にあるpaneの
                //splitPane情報の自身を参照する方向を自身をまたいだ方向にあるpaneに差し替える
                foreach (var pLayoutEngine in GetChildrenOf(paneRmvAlign, Children.IndexOf(pane) + 1))
                    switch (paneRmvAlign)
                    {
                        case DockDirection.Top:
                            pLayoutEngine.Bottom = pane.Bottom;
                            break;
                        case DockDirection.Bottom:
                            pLayoutEngine.Top = pane.Top;
                            break;
                        case DockDirection.Left:
                            pLayoutEngine.Right = pane.Right;
                            break;
                        case DockDirection.Right:
                            pLayoutEngine.Left = pane.Left;
                            break;
                    }
            }
            base.Remove(pane);
        }
        public override DockBaseConfig GetConfig()
        {
            var conf = new DockPaneBaseConfig();
            InitConfig(conf);
            return conf;
        }
        protected override void InitConfig(DockBaseConfig config)
        {
            var conf = (DockPaneBaseConfig)config;
            conf.Target = this;
            conf.Index = OwnNodes.IndexOf(this);
            conf.Align = Align;
            conf.Size = Length;
            Target.InternalInitConfig(conf);

            base.InitConfig(config);
        }
        public DockDirection GetSucceedingDireWhenDeletingPane()
        {
            if (Children.Count > 0)
                return Children.Last().Align;
            if (((Parent is DockBayLayoutEngine) && (this == Parent.Children[0])) && Parent.Children.Count > 1)
                return Parent.Children[1].Align;

            switch (Align)
            {
                case DockDirection.None:
                    throw new System.ComponentModel.InvalidEnumArgumentException(
                        String.Format("\"{0}\"は引数として使用できません", Align.ToString()));
                default:
                    return ~Align;
            }
        }

        void Initialize(DockNodeLayoutEngine parent, DockBayLayoutEngine owner)
        {
            Owner = owner;
            Parent = parent;
            foreach (var p in Children)
                p.Initialize(this, owner);
        }
        internal void Initialize(DockNodeLayoutEngine parent, DockBayLayoutEngine owner, DockDirection align)
        {
            Initialize(parent, owner);
            Align = align;
        }
        internal override IEnumerable<DockPaneLayoutEngine> GetChildrenOf(DockDirection align, int skip = 0, bool deep = true)
        {
            var flg = DockDirection.None;
            var enumer = Children
                .Skip(skip)
                .Where(n =>
                {
                    //初回実行にはflgを初期化
                    if (Children[skip] == n)
                        flg = DockDirection.None;
                    return n.Align != ~align;
                })
                .TakeWhile(n =>
                {
                    var tmp = flg != align;
                    if (tmp)
                        flg = n.Align;
                    return tmp;
                });
            if (deep)
                enumer = enumer.SelectMany(n => n.GetChildrenOf(align));
            enumer = enumer.Concat(new object[] { null }
                .Where(o => flg != align)
                .Select(b => this));
            return enumer;
        }
        internal override void AddEngine(DockPaneLayoutEngine layout)
        {
            base.AddEngine(layout);
            if (Parent != null)
                Parent.AddEngine(layout);
        }
        internal override void RemoveEngine(DockPaneLayoutEngine layout)
        {
            base.RemoveEngine(layout);
            if (Parent != null)
                Parent.RemoveEngine(layout);
        }

        protected override void OnPaneReplaced(LayoutEngineReplaceEventArgs e)
        {
            base.OnPaneReplaced(e);
            e.NewPane.Align = e.OldPane.Align;
            e.NewPane.Length = e.OldPane.Length;
        }
        public event EventHandler Removed;
        internal virtual void OnRemoved(EventArgs e)
        {
            SplitterVisible = true;
            if (Removed != null)
                Removed(this, e);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
    public enum VisibleMode
    {
        Visible, Fill, Hidden,
    }
}
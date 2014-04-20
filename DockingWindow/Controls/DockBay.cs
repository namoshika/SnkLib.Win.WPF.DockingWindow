using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SunokoLibrary.Windows.Controls
{
    public class DockBay : DockFloatableBay
    {
        public DockBay()
        {
            _baysOrder = new LinkedList<DockFloatableBay>();
            FloatingBays = new ObservableCollection<DockFloatingBay>();
            FloatingBays.CollectionChanged += FloatingBays_CollectionChanged;
            Loaded += DockBay_Loaded;
            Unloaded += DockBay_Unloaded;
        }

        DockPaneBase _hoverPane;
        DockFloatableBay _hoverBay;
        DockFloatingBay _dragingBay;
        DragDropEffects _dragEffect;
        LinkedList<DockFloatableBay> _baysOrder;

        public ObservableCollection<DockFloatingBay> FloatingBays
        {
            get { return (ObservableCollection<DockFloatingBay>)GetValue(FloatingBaysProperty); }
            protected set { SetValue(FloatingBaysProperty, value); }
        }
        protected DockingHelper DockingHelper { get; private set; }
        public override DockFloatingBay PurgePane(DockPane pane, Point windowLocation, Size windowSize)
        {
            var bay = new DockFloatingBay();
            bay.Items.Add(pane);
            bay.WindowTop = windowLocation.Y;
            bay.WindowLeft = windowLocation.X;
            bay.WindowWidth = windowSize.Width;
            bay.WindowHeight = windowSize.Height;
            FloatingBays.Add(bay);

            return bay;
        }

        void DockBay_Loaded(object sender, RoutedEventArgs e)
        {
            DockingHelper = new DockingHelper();
            DockingHelper.Owner = Window.GetWindow(this);
            DockingHelper.Visibility = Visibility.Collapsed;
            DockingHelper.Width = 0;
            DockingHelper.Height = 0;
            DockingHelper.Show();
            DockingHelper.Topmost = true;
        }
        void DockBay_Unloaded(object sender, RoutedEventArgs e)
        {
            DockingHelper.Close();
        }
        void wnd_Activated(object sender, EventArgs e)
        {
            var wnd = (DockFloatingWindow)sender;
            var bay = wnd.DockBay;
            _baysOrder.Remove(bay);
            _baysOrder.AddFirst(bay);
        }
        void wnd_Closed(object sender, EventArgs e)
        {
            var wnd = (DockFloatingWindow)sender;
            FloatingBays.Remove(wnd.DockBay);
        }
        void wnd_WindowDraged(object sender, EventArgs e)
        {
            //Helperを表示するBayがあるかどうかを調べる
            var mvForm = (DockFloatingWindow)sender;
            var hoverBay = _baysOrder.Concat(new[] { this }).Where(bay =>
                {
                    var mp = Mouse.GetPosition(bay);
                    return bay != mvForm.DockBay &&
                        (mp.X >= 0 && mp.X < bay.RenderSize.Width && mp.Y >= 0 && mp.Y < bay.RenderSize.Height);
                }).FirstOrDefault();
            _dragingBay = mvForm.DockBay;
            _hoverBay = hoverBay;

            //Helperを表示する必要がある場合は表示とマウスが乗ってる座標下の
            //Paneも探す。無い場合は非表示にし、各一時保存用変数をクリアする
            if (hoverBay != null)
            {
                var mPos = Mouse.GetPosition(hoverBay);
                var childAtPoint = hoverBay.GetChildAtPoint(mPos);
                _dragEffect = DockingHelper.GetDragDropEffect(hoverBay, childAtPoint);
                _hoverPane = childAtPoint;
                DockingHelper.ActiveIndicator = _dragEffect;
            }
            else
            {
                DockingHelper.Visibility = System.Windows.Visibility.Hidden;
                _dragEffect = DragDropEffects.None;
                _hoverPane = null;
                DockingHelper.ActiveIndicator = _dragEffect;
            }
            OnFloatingWindowMoved(new FloatFormEventArgs((DockFloatingWindow)sender));
        }
        void wnd_EndWindowDrag(object sender, EventArgs e)
        {
            if (_dragingBay == null)
                return;

            var len = _dragingBay.Items.Count == 1 && ((DockPaneBase)_dragingBay.Items[0]).Items.Count == 0
                ? ((DockPaneBase)_dragingBay.Items[0]).Length : new DockLength();
            switch (_dragEffect)
            {
                case DragDropEffects.Top:
                    _hoverPane.AddPanesOf(_dragingBay, DockDirection.Top, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.Bottom:
                    _hoverPane.AddPanesOf(_dragingBay, DockDirection.Bottom, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.Left:
                    _hoverPane.AddPanesOf(_dragingBay, DockDirection.Left, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.Right:
                    _hoverPane.AddPanesOf(_dragingBay, DockDirection.Right, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.Fill:
                    break;
                case DragDropEffects.OuterTop:
                    _hoverBay.AddPanesOf(_dragingBay, DockDirection.Top, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.OuterBottom:
                    _hoverBay.AddPanesOf(_dragingBay, DockDirection.Bottom, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.OuterLeft:
                    _hoverBay.AddPanesOf(_dragingBay, DockDirection.Left, len);
                    Window.GetWindow(this).Activate();
                    break;
                case DragDropEffects.OuterRight:
                    _hoverBay.AddPanesOf(_dragingBay, DockDirection.Right, len);
                    Window.GetWindow(this).Activate();
                    break;
                default:
                    break;
            }

            DockingHelper.Visibility = System.Windows.Visibility.Hidden;
            _hoverPane = null;
            _hoverBay = null;
            _dragingBay = null;
            _dragEffect = DragDropEffects.None;

            OnFloatingWindowEndMove(new FloatFormEventArgs((DockFloatingWindow)sender));
        }
        void FloatingBays_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var bays = (ObservableCollection<DockFloatingBay>)sender;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (DockFloatingBay item in bays)
                    {
                        item.TitleBarHeight = TitleBarHeight;
                        item.Initialize(this, (DockBayLayoutEngine)LayoutEngine);
                        _baysOrder.AddFirst(item);

                        var wnd = new DockFloatingWindow(item);
                        wnd.Closed += wnd_Closed;
                        wnd.DragedWindowMoved += wnd_WindowDraged;
                        wnd.EndWindowDrag += wnd_EndWindowDrag;
                        wnd.Activated += wnd_Activated;
                        wnd.Show();
                        wnd.Owner = Window.GetWindow(this);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (DockFloatingBay item in e.NewItems)
                    {
                        item.TitleBarHeight = TitleBarHeight;
                        item.Initialize(this, (DockBayLayoutEngine)LayoutEngine);
                        _baysOrder.AddFirst(item);

                        var wnd = new DockFloatingWindow(item);
                        wnd.Closed += wnd_Closed;
                        wnd.DragedWindowMoved += wnd_WindowDraged;
                        wnd.EndWindowDrag += wnd_EndWindowDrag;
                        wnd.Activated += wnd_Activated;
                        wnd.Show();
                        wnd.Owner = Window.GetWindow(this);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (DockFloatingBay item in e.OldItems)
                    {
                        //これやるとRootがnullになるためにPanePurge時にRootへ参照できな
                        //くなる。Closeしたら再利用不可だしRootは後処理せずに放っておく
                        //item.Initialize(null);
                        _baysOrder.Remove(item);

                        var wnd = (DockFloatingWindow)item.Parent;
                        wnd.Closed -= wnd_Closed;
                        wnd.DragedWindowMoved -= wnd_WindowDraged;
                        wnd.EndWindowDrag -= wnd_EndWindowDrag;
                        wnd.Activated -= wnd_Activated;
                        wnd.Close();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public event FloatFormEventHandler FloatingWindowMoved;
        protected void OnFloatingWindowMoved(FloatFormEventArgs e)
        {
            if (FloatingWindowMoved != null)
                FloatingWindowMoved(this, e);
        }
        public event FloatFormEventHandler FloatingWindowEndMove;
        protected void OnFloatingWindowEndMove(FloatFormEventArgs e)
        {
            if (FloatingWindowEndMove != null)
                FloatingWindowEndMove(this, e);
        }

        public static readonly DependencyProperty FloatingBaysProperty = DependencyProperty.Register(
            "FloatingBays", typeof(ObservableCollection<DockFloatingBay>), typeof(DockBay),
            new UIPropertyMetadata(new ObservableCollection<DockFloatingBay>()));
    }
    [Serializable]
    public class DockBayConfig : DockBayBaseConfig
    {
        public DockBayConfig()
        {
            FloatingBays = new List<DockBayFloatingConfig>();
        }

        [XmlArrayItem(Type = typeof(DockBayFloatingConfig))]
        public List<DockBayFloatingConfig> FloatingBays { get; set; }
    }
    public delegate void FloatFormEventHandler(object sender, FloatFormEventArgs e);
    public class FloatFormEventArgs
    {
        public FloatFormEventArgs(DockFloatingWindow form)
        {
            FloatForm = form;
        }
        public DockFloatingWindow FloatForm { get; protected set; }
    }
}
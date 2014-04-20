using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SunokoLibrary.Windows.Controls
{
    public class DockFloatingBay : DockFloatableBay
    {
        public DockFloatingBay()
        {
            _layout = new DockFloatingBayLayoutEngine(this);
            _layout.ChangedRootBay += _layout_ChangedRootBay;
            _layout.Closed += _layout_Closed;
            CommandBindings.Add(new System.Windows.Input.CommandBinding(
                DockCommands.CloseFloatForm, BayClosedCmd_Executed));

            //_helper = new DockingHelper();
            //_helper.IndicatorStyle = HelperModes.InnerHelperOnly;
            //_helper.FormDragDrop += _helper_FormDragDrop;
            //_helper.Width = 0;
            //_helper.Height = 0;
            //_helper.Show();
            //_helper.Visibility = Visibility.Collapsed;
        }

        DockBay _rootBay;
        DockFloatingBayLayoutEngine _layout;

        public double WindowLeft
        {
            get { return (double)GetValue(WindowLeftProperty); }
            set { SetValue(WindowLeftProperty, value); }
        }
        public double WindowTop
        {
            get { return (double)GetValue(WindowTopProperty); }
            set { SetValue(WindowTopProperty, value); }
        }
        public double WindowWidth
        {
            get { return (double)GetValue(WindowWidthProperty); }
            set { SetValue(WindowWidthProperty, value); }
        }
        public double WindowHeight
        {
            get { return (double)GetValue(WindowHeightProperty); }
            set { SetValue(WindowHeightProperty, value); }
        }
        protected override DockNodeLayoutEngine LayoutEngine
        { get { return _layout; } }

        public override DockFloatingBay PurgePane(DockPane pane, Point windowLocation, Size windowSize)
        {
            if (_layout.Root == null)
                throw new InvalidOperationException("LayoutEngine.Rootがnullの状態でPurgeすることはできません。");

            var root = (DockBay)_layout.Root.Target;
            return root.PurgePane(pane, windowLocation, windowSize);
        }
        internal void Initialize(DockBay rootBay, DockBayLayoutEngine root)
        {
            _rootBay = rootBay;
            _layout.Root = root;
        }
        void BayClosedCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            //_layout_Closedが呼び出された時でも、form側のDockFloatingWindow_Closedが
            //呼び出されてこのメソッドが呼び出される。そのため、無駄に呼び出さないよう
            //にif文を付ける
            if (!_layout.IsClosed)
                _layout.Close();
        }
        void _layout_Closed(object sender, EventArgs e)
        {
            DockCommands.CloseFloatForm.Execute(null, Parent as IInputElement);
            OnClosed(new EventArgs());
        }
        void _layout_ChangedRootBay(object sender, EventArgs e)
        {
            //_rootBay = rootBay;
        }

        public event EventHandler Closed;
        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        public static readonly DependencyProperty WindowLeftProperty = DependencyProperty.Register(
            "WindowLeft", typeof(double), typeof(DockFloatingBay), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty WindowTopProperty = DependencyProperty.Register(
            "WindowTop", typeof(double), typeof(DockFloatingBay), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register(
            "WindowWidth", typeof(double), typeof(DockFloatingBay), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register(
            "WindowHeight", typeof(double), typeof(DockFloatingBay), new UIPropertyMetadata(0.0));
    }
    public class DockFloatingBayLayoutEngine : DockBayLayoutEngine
    {
        public DockFloatingBayLayoutEngine(DockFloatingBay target)
            : base(target)
        {
            IsClosed = false;
            PaneRemoved += DockFloatingBayLayoutEngine_PaneRemoved;
        }
        DockBayLayoutEngine _root;

        public bool IsClosed { get; protected set; }
        public DockBayLayoutEngine Root
        {
            get { return _root; }
            set
            {
                if (_root == value)
                    return;
                _root = value;
                OnChangedRootBay(new EventArgs());
            }
        }
        public void Close()
        {
            if (IsClosed)
                return;

            IsClosed = true;
            for (var i = 0; i < DockPanes.Count; i++)
                DockPanes[0].Parent.Remove(DockPanes[0]);
        }

        void DockFloatingBayLayoutEngine_PaneRemoved(object sender, LayoutEngineEventArgs e)
        {
            if (Children.Count == 0)
                OnClosed(new EventArgs());
        }

        public event EventHandler ChangedRootBay;
        protected virtual void OnChangedRootBay(EventArgs e)
        {
            if (ChangedRootBay != null)
                ChangedRootBay(this, e);
        }
        public event EventHandler Closed;
        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
    }

    [Serializable]
    public class DockBayFloatingConfig : DockBayBaseConfig
    {
        public DockBayFloatingConfig()
        {
            IndexInRootBay = new List<int>();
        }

        public Point Location { get; set; }
        public Size Size { get; set; }
        public List<int> IndexInRootBay { get; set; }
    }
}
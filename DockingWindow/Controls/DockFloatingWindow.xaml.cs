using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SunokoLibrary.Windows.Controls
{
    /// <summary>
    /// DockFloatingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DockFloatingWindow : Window
    {
        public DockFloatingWindow(DockFloatingBay bay)
        {
            InitializeComponent();
            _isDraging = false;
            Loaded += DockFloatingWindow_Loaded;
            Closed += DockFloatingWindow_Closed;
            MouseMove += DockFloatingWindow_MouseMove;
            MouseUp += DockFloatingWindow_MouseUp;
            Content = bay;
            DockBay = bay;
            ShowInTaskbar = false;
            RegisterName("bay", bay);
        }

        bool _isDraging;
        double _offset;
        public DockFloatingBay DockBay { get; protected set; }

        public void HockMousePointer()
        { HockMousePointer(ActualWidth / 2); }
        public void HockMousePointer(double offset)
        {
            _isDraging = true;
            _offset = offset;
            CaptureMouse();
        }
        public void UnHockMousePointer()
        {
            _isDraging = false;
            ReleaseMouseCapture();
            OnEndWindowDrag(new EventArgs());
        }

        void DockFloatingWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && _isDraging)
                UnHockMousePointer();

            if (_isDraging)
            {
                var dragOffset = _offset;
                var mpScreen = PointToScreen(Mouse.GetPosition(this));
                Left = mpScreen.X - dragOffset - (SystemParameters.FixedFrameVerticalBorderWidth + SystemParameters.BorderWidth);
                Top = mpScreen.Y - (SystemParameters.SmallCaptionHeight / 2);

                OnDragedWindowMoved(new EventArgs());
            }
        }
        void DockFloatingWindow_MouseUp(object sender, MouseButtonEventArgs e)
        { UnHockMousePointer(); }
        void DockFloatingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = System.Windows.Interop.HwndSource.FromHwnd(
                new System.Windows.Interop.WindowInteropHelper(this).Handle);
            hwnd.AddHook(WndProc);
        }
        void DockFloatingWindow_Closed(object sender, EventArgs e)
        { DockCommands.CloseFloatForm.Execute(null, Content as IInputElement); }
        void BayClosedCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //form.closed時にもVM側を経由し、これが呼び出される。ifで無駄にメソッドを
            //呼び出すのを避けたいが、開いてるかどうかを判定する方法がないので放置する。
            //一応、close()は複数回呼び出しをしても壊れない。
            Close();
        }
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Console.WriteLine(Convert.ToString(msg, 16));
            switch (msg)
            {
                case 0x216:
                    //OnDragedWindowMoved(new EventArgs());
                    //handled = true;
                    break;
                case 0x232:
                    //OnEndWindowDrag(new EventArgs());
                    //handled = true;
                    break;
                case 0x112:
                    var p = wParam.ToInt32();
                    switch (p & 0xFFF0)
                    {
                        case 0xF010:
                            //Windowドラッグをキャンセルし、自前のドラッグ機構を使用する
                            //これによってドラッグ中でもマウス座標を取得できるようにする
                            Mouse.Capture(this);
                            var mp = Mouse.GetPosition(this);
                            HockMousePointer(mp.X);

                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public event EventHandler DragedWindowMoved;
        protected void OnDragedWindowMoved(EventArgs e)
        {
            if (DragedWindowMoved != null)
                DragedWindowMoved(this, e);
        }
        public event EventHandler EndWindowDrag;
        protected void OnEndWindowDrag(EventArgs e)
        {
            if (EndWindowDrag != null)
                EndWindowDrag(this, e);
        }
    }
}
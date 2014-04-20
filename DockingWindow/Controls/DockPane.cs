using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Input;

namespace SunokoLibrary.Windows.Controls
{
    public class DockPane : DockPaneBase
    {
        public DockPane()
        {
            CommandBindings.AddRange(
                new CommandBinding[]
                {
                    new CommandBinding(DockCommands.PurgeAndAttachMouse, cmdBinding_PurgeDockPane)
                });
        }
        static DockPane()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DockPane), new FrameworkPropertyMetadata(typeof(DockPane)));
        }

        public string Header
        {
            get { return GetValue(HeaderProperty) as string; }
            set { SetValue(HeaderProperty, value); }
        }
        public bool TitleBarVisible
        {
            get { return (bool)GetValue(TitleBarVisibleProperty); }
            set { SetValue(TitleBarVisibleProperty, value); }
        }
        public double TitleBarHeight
        {
            get { return DockFloatableBay.GetTitleBarHeight(this); }
            set { DockFloatableBay.SetTitleBarHeight(this, value); }
        }

        void cmdBinding_CloseDockPane(object sender, ExecutedRoutedEventArgs e)
        {
            if (((DockPaneLayoutEngine)LayoutEngine).Parent == null)
                return;

            ((DockPaneLayoutEngine)LayoutEngine).Parent.Remove((DockPaneLayoutEngine)LayoutEngine);
        }
        void cmdBinding_PurgeDockPane(object sender, ExecutedRoutedEventArgs e)
        {
            if (((DockPaneLayoutEngine)LayoutEngine).Owner == null)
                return;
            if (((DockPaneLayoutEngine)LayoutEngine).Owner.Target is DockFloatableBay == false)
                return;

            var bay = (DockFloatableBay)((DockPaneLayoutEngine)LayoutEngine).Owner.Target;
            var mpScreen =PointToScreen(Mouse.GetPosition(this));
            var size = new Size(ContentWidth, ContentHeight);
            var location = new Point(
                mpScreen.X - ActualWidth / 2 - (SystemParameters.FixedFrameVerticalBorderWidth + SystemParameters.BorderWidth),
                mpScreen.Y - (SystemParameters.SmallCaptionHeight / 2));

            ((DockNode)Parent).Items.Remove(this);
            Align = DockDirection.Top;
            var form = bay.PurgePane(this, location, size);

            ((DockFloatingWindow)form.Parent).HockMousePointer();
        }

        public static DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(DockPane), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty TitleBarVisibleProperty = DependencyProperty.Register(
            "TitleBarVisible", typeof(bool), typeof(DockPane), new UIPropertyMetadata(true));
    }
}
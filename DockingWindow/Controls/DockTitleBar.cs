using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SunokoLibrary.Windows.Controls
{
    public class DockTitleBar : HeaderedItemsControl
    {
        static DockTitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockTitleBar), new FrameworkPropertyMetadata(typeof(DockTitleBar)));
        }
        double _purge_moveDistance = 10;
        Point _moveDistance;
        Thumb _thumb;

        public ICommand Command
        {
            get { return GetValue(CommandProperty) as ICommand; }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public IInputElement CommandTarget
        {
            get { return GetValue(CommandTargetProperty) as IInputElement; }
            set { SetValue(CommandTargetProperty, value); }
        }

        //TitleBar Floating動作実装
        void dragBar_DragStarted(object sender, DragStartedEventArgs e)
        {
            _moveDistance = new Point(0, 0);
        }
        void dragBar_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_moveDistance.X == double.NaN || _moveDistance.Y == double.NaN)
                return;

            _moveDistance.X += e.HorizontalChange;
            _moveDistance.Y += e.VerticalChange;
            var mvDstnce = Math.Sqrt(Math.Pow(_moveDistance.X, 2) + Math.Pow(_moveDistance.Y, 2));
            if (mvDstnce >= _purge_moveDistance && Command != null)
            {
                Console.WriteLine(mvDstnce);
                _moveDistance = new Point(double.NaN, double.NaN);
                if (Command is RoutedCommand)
                    ((RoutedCommand)Command).Execute(
                        CommandParameter, CommandTarget == null ? this : CommandTarget);
                else
                    Command.Execute(CommandParameter);
            }
        }
        void dragBar_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _moveDistance = new Point(double.NaN, double.NaN);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var ele = Template.FindName("thumb", this);
            var bar = ele as Thumb;
            if (ele == null || bar == null)
                throw new NotSupportedException("掴めません。");

            _thumb = bar;
            _thumb.DragStarted += dragBar_DragStarted;
            _thumb.DragDelta += dragBar_DragDelta;
            _thumb.DragCompleted += dragBar_DragCompleted;
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(DockTitleBar));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(DockTitleBar));
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget", typeof(IInputElement), typeof(DockTitleBar));
    }
}
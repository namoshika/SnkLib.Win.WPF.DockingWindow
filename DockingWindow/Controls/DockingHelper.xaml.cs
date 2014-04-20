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
    /// DockingHelper.xaml の相互作用ロジック
    /// </summary>
    public partial class DockingHelper : Window
    {
        public DockingHelper() { InitializeComponent(); }

        public HelperModes IndicatorMode
        {
            get { return (HelperModes)GetValue(IndicatorModeProperty); }
            set { SetValue(IndicatorModeProperty, value); }
        }
        public DragDropEffects ActiveIndicator
        {
            get { return (DragDropEffects)GetValue(ActiveIndicatorProperty); }
            set { SetValue(ActiveIndicatorProperty, value); }
        }
        /// <summary>DockingHelperを表示し、マウスのイテレータへの当たり判定を行います。</summary>
        /// <param name="bay">DockingHelperを重ねるDockBayBase</param>
        /// <param name="pane">PaneHelperを重ねるDockPaneBase</param>
        /// <returns>マウスオーバーされてるイテレータ</returns>
        public DragDropEffects GetDragDropEffect(DockBayBase bay, DockPaneBase pane)
        {
            //引数bayへDockingHelperを重ねる
            var rect = new Rect(bay.PointToScreen(new Point(0, 0)), bay.RenderSize);
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
            Visibility = Visibility.Visible;

            if (pane != null)
            {
                //引数paneの中央へ重なるようにPaneHelperを移動させる
                var p = pane.TranslatePoint(new Point(pane.ContentLeft, pane.ContentTop), this);
                PaneHelper.Margin = new Thickness(
                    ((pane.ContentWidth - PaneHelper.RenderSize.Width) / 2) + p.X,
                    ((pane.ContentHeight - PaneHelper.RenderSize.Height) / 2) + p.Y,
                    0, 0);
            }

            //マウスポインタ座標からDragDropEffectsを求める
            var pt = Mouse.GetPosition(this);
            var res = (new[]
            {
                new {Ind = (FrameworkElement)InTopIndicator, Eff = DragDropEffects.Top},
                new {Ind = (FrameworkElement)InBottomIndicator, Eff = DragDropEffects.Bottom},
                new {Ind = (FrameworkElement)InLeftIndicator, Eff = DragDropEffects.Left},
                new {Ind = (FrameworkElement)InRightIndicator, Eff = DragDropEffects.Right},
                new {Ind = (FrameworkElement)OutTopIndicator, Eff = DragDropEffects.OuterTop},
                new {Ind = (FrameworkElement)OutBottomIndicator, Eff = DragDropEffects.OuterBottom},
                new {Ind = (FrameworkElement)OutLeftIndicator, Eff = DragDropEffects.OuterLeft},
                new {Ind = (FrameworkElement)OutRightIndicator, Eff = DragDropEffects.OuterRight},
                new {Ind = (FrameworkElement)InCenterIndicator, Eff = DragDropEffects.Fill},
                new {Ind = (FrameworkElement)bay, Eff = DragDropEffects.Enter},
            })
            .Where(pair =>
                pair.Ind.Visibility == System.Windows.Visibility.Visible &&
                new Rect(pair.Ind.TranslatePoint(new Point(), this), pair.Ind.RenderSize).Contains(pt))
            .Select(pair => pair.Eff)
            .Concat(new[] { DragDropEffects.None })
            .First();

            return res;
        }

        public static readonly DependencyProperty IndicatorModeProperty = DependencyProperty.Register(
            "IndicatorMode", typeof(HelperModes), typeof(DockingHelper),
            new UIPropertyMetadata(HelperModes.Both));
        public static readonly DependencyProperty ActiveIndicatorProperty =
            DependencyProperty.Register("ActiveIndicator", typeof(DragDropEffects), typeof(DockingHelper),
            new UIPropertyMetadata(DragDropEffects.None, (sender, e) =>
            {
                var obj = (DockingHelper)sender;
                var value = (DragDropEffects)e.NewValue;
                obj.InCenterIndicator.IsActive = value == DragDropEffects.Fill;
                obj.InTopIndicator.IsActive = value == DragDropEffects.Top;
                obj.InBottomIndicator.IsActive = value == DragDropEffects.Bottom;
                obj.InLeftIndicator.IsActive = value == DragDropEffects.Left;
                obj.InRightIndicator.IsActive = value == DragDropEffects.Right;
                obj.OutTopIndicator.IsActive = value == DragDropEffects.OuterTop;
                obj.OutBottomIndicator.IsActive = value == DragDropEffects.OuterBottom;
                obj.OutLeftIndicator.IsActive = value == DragDropEffects.OuterLeft;
                obj.OutRightIndicator.IsActive = value == DragDropEffects.OuterRight;
                obj.PaneHelper.Visibility = value != DragDropEffects.None ? Visibility.Visible : Visibility.Hidden;
            }));
    }
    public class ConvertHelperModesToVisibility : System.Windows.Data.IValueConverter
    {
        public bool IsInner { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (HelperModes)value;
            return val == HelperModes.Both || (val == HelperModes.InnerHelperOnly && IsInner || val == HelperModes.OuterHelperOnly && !IsInner)
                ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum HelperModes
    {
        Hide,
        InnerHelperOnly,
        OuterHelperOnly,
        Both
    }
    public enum DragDropEffects
    {
        None, Enter,
        Top, Bottom, Left, Right, Fill,
        OuterTop, OuterBottom, OuterLeft, OuterRight
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SunokoLibrary.Windows.Controls.Primitives
{
    public class EnclosePanel : Panel
    {
        static EnclosePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnclosePanel), new FrameworkPropertyMetadata(typeof(EnclosePanel)));
        }

        public double CenterTop
        {
            get { return (double)GetValue(CenterTopProperty); }
            set { SetValue(CenterTopProperty, value); }
        }
        public double CenterLeft
        {
            get { return (double)GetValue(CenterLeftProperty); }
            set { SetValue(CenterLeftProperty, value); }
        }
        public double CenterWidth
        {
            get { return (double)GetValue(CenterWidthProperty); }
            set { SetValue(CenterWidthProperty, value); }
        }
        public double CenterHeight
        {
            get { return (double)GetValue(CenterHeightProperty); }
            set { SetValue(CenterHeightProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double wdth = availableSize.Width, hght = availableSize.Height;
            foreach (DockPaneBase item in Children)
            {
                var align = item.Align;
                double size;
                switch (align)
                {
                    case DockDirection.Top:
                        size = Math.Min(hght * item.Length.Rate, item.Length.Pixel);
                        item.Measure(new Size(wdth, size));
                        hght -= size;
                        break;
                    case DockDirection.Bottom:
                        size = Math.Min(hght * item.Length.Rate, item.Length.Pixel);
                        item.Measure(new Size(wdth, size));
                        hght -= size;
                        break;
                    case DockDirection.Left:
                        size = Math.Min(wdth * item.Length.Rate, item.Length.Pixel);
                        item.Measure(new Size(size, hght));
                        wdth -= size;
                        break;
                    case DockDirection.Right:
                        size = Math.Min(wdth * item.Length.Rate, item.Length.Pixel);
                        item.Measure(new Size(size, hght));
                        wdth -= size;
                        break;
                    default:
#if DEBUG
                        System.Diagnostics.Debug.Fail("仕様外の状態です。");
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                }
            }
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = base.ArrangeOverride(finalSize);
            double top = 0, lft = 0, wdth = rect.Width, hght = rect.Height;
            foreach (DockPaneBase item in Children)
            {
                var align = item.Align;
                double size;
                switch (align)
                {
                    case DockDirection.Top:
                        size = item.DesiredSize.Height;
                        item.Arrange(new Rect(lft, top, Math.Max(wdth, 0), Math.Max(size, 0)));
                        top += size;
                        hght -= size;
                        break;
                    case DockDirection.Bottom:
                        size = item.DesiredSize.Height;
                        item.Arrange(new Rect(lft, top + hght - size, Math.Max(wdth, 0), Math.Max(size, 0)));
                        hght -= size;
                        break;
                    case DockDirection.Left:
                        size = item.DesiredSize.Width;
                        item.Arrange(new Rect(lft, top, Math.Max(size, 0), Math.Max(hght, 0)));
                        lft += size;
                        wdth -= size;
                        break;
                    case DockDirection.Right:
                        size = item.DesiredSize.Width;
                        item.Arrange(new Rect(lft + wdth - size, top, Math.Max(size, 0), Math.Max(hght, 0)));
                        wdth -= size;
                        break;
                    default:
#if DEBUG
                        System.Diagnostics.Debug.Fail("仕様外の状態です。");
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                }
            }
            CenterTop = top;
            CenterLeft = lft;
            CenterWidth = wdth;
            CenterHeight = hght;

            return finalSize;
        }
        
        public static readonly DependencyProperty CenterTopProperty = DependencyProperty.RegisterAttached(
            "CenterTop", typeof(double), typeof(EnclosePanel), new UIPropertyMetadata(double.NaN));
        public static readonly DependencyProperty CenterLeftProperty = DependencyProperty.RegisterAttached(
            "CenterLeft", typeof(double), typeof(EnclosePanel), new UIPropertyMetadata(double.NaN));
        public static readonly DependencyProperty CenterWidthProperty = DependencyProperty.RegisterAttached(
            "CenterWidth", typeof(double), typeof(EnclosePanel), new UIPropertyMetadata(double.NaN));
        public static readonly DependencyProperty CenterHeightProperty = DependencyProperty.RegisterAttached(
            "CenterHeight", typeof(double), typeof(EnclosePanel), new UIPropertyMetadata(double.NaN));
    }
}
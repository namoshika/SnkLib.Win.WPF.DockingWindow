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
    public class ExDockPanel : Panel
    {
        static ExDockPanel()
        { DefaultStyleKeyProperty.OverrideMetadata(typeof(ExDockPanel), new FrameworkPropertyMetadata(typeof(ExDockPanel))); }

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = availableSize.Width;
            var height = availableSize.Height;
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                var item = (DockPaneBase)Children[i];
                var align = item.Align;
                double size;
                if (i == 0)
                    item.Measure(new Size(width, height));
                else
                    switch (align)
                    {
                        case DockDirection.Top:
                        case DockDirection.Bottom:
                            size = Math.Min(height * item.Length.Rate, item.Length.Pixel);
                            item.Measure(new Size(width, size));
                            height -= size;
                            break;
                        case DockDirection.Left:
                        case DockDirection.Right:
                            size = Math.Min(width * item.Length.Rate, item.Length.Pixel);
                            item.Measure(new Size(size, height));
                            width -= size;
                            break;
                    }
            }
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = base.ArrangeOverride(finalSize);
            double top = 0, lft = 0, wdth = rect.Width, hght = rect.Height;
            for(var i = Children.Count - 1; i >= 0; i--)
            {
                var item = (DockPaneBase)Children[i];
                var align = item.Align;
                double size;
                if (i == 0)
                    item.Arrange(new Rect(lft, top, wdth, hght));
                else
                    switch (align)
                    {
                        case DockDirection.Top:
                            size = item.DesiredSize.Height;
                            item.Arrange(new Rect(lft, top, wdth, size));
                            top += size;
                            hght -= size;
                            break;
                        case DockDirection.Bottom:
                            size = item.DesiredSize.Height;
                            item.Arrange(new Rect(lft, top + hght - size, wdth, size));
                            hght -= size;
                            break;
                        case DockDirection.Left:
                            size = item.DesiredSize.Width;
                            item.Arrange(new Rect(lft, top, size, hght));
                            lft += size;
                            wdth -= size;
                            break;
                        case DockDirection.Right:
                            size = item.DesiredSize.Width;
                            item.Arrange(new Rect(lft + wdth - size, top, size, hght));
                            wdth -= size;
                            break;
                    }
            }
            return finalSize;
        }
    }
}
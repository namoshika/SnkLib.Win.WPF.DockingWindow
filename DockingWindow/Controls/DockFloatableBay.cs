using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SunokoLibrary.Windows.Controls
{
    public abstract class DockFloatableBay : DockBayBase
    {
        static DockFloatableBay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DockFloatableBay), new FrameworkPropertyMetadata(typeof(DockFloatableBay)));
        }

        public double TitleBarHeight
        {
            get { return (double)GetTitleBarHeight(this); }
            set { SetTitleBarHeight(this, value); }
        }
        public abstract DockFloatingBay PurgePane(DockPane pane, Point windowLocation, Size windowSize);
        internal DockPaneBase GetChildAtPoint(Point point)
        {
            var pane = ((DockBayLayoutEngine)LayoutEngine).OwnNodes
                .Select(l => (DockPaneBase)l.Target).FirstOrDefault(p =>
            {
                var pt = p.TranslatePoint(new Point(p.ContentLeft, p.ContentTop), this);
                var rect = new Rect(pt, new Size(p.ContentWidth, p.ContentHeight));
                return rect.Contains(point);
            });
            return pane;
        }

        public static double GetTitleBarHeight(DependencyObject obj)
        { return (double)obj.GetValue(TitleBarHeightProperty); }
        public static void SetTitleBarHeight(DependencyObject obj, double value)
        { obj.SetValue(TitleBarHeightProperty, value); }
        public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.RegisterAttached(
            "TitleBarHeight", typeof(double), typeof(DockFloatableBay), new FrameworkPropertyMetadata(
                20.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));
    }
}
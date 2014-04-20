using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SunokoLibrary.Windows.Controls
{
    public class DockBayBase : DockNode
    {
        public DockBayBase()
        {
            _layout = new DockBayLayoutEngine(this);
        }
        static DockBayBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DockBayBase), new FrameworkPropertyMetadata(typeof(DockBayBase)));
        }
        DockBayLayoutEngine _layout;

        public double SplitterWidth
        {
            get { return (double)GetValue(SplitterWidthProperty); }
            set { SetValue(SplitterWidthProperty, value); }
        }
        public System.Windows.Media.Brush SplitterBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(SplitterBrushProperty); }
            set { SetValue(SplitterBrushProperty, value); }
        }
        protected override DockNodeLayoutEngine LayoutEngine { get { return _layout; } }
        internal override Size ItemsPanelSize
        {
            get
            {
                return new Size(
                    ActualWidth - (BorderThickness.Left + BorderThickness.Right),
                    ActualHeight - (BorderThickness.Top + BorderThickness.Bottom));
            }
        }
        internal override void InternalApplyConfig(DockBaseConfig config)
        {
            while (LayoutEngine.OwnNodes.Count != 0)
            {
                var pane = (DockPaneBase)LayoutEngine.OwnNodes.First().Target;
                var parent = (DockNode)pane.Parent;
                parent.Items.Remove(pane);
            }
            foreach (var c in config.Children)
            {
                var p = (DockPaneBase)c.Target.Target;
                p.Name = c.Name;
                p.Length = c.Size;
                p.Align = c.Align;
                Items.Add(c.Target.Target);
            }
        }
        internal override void InternalInitConfig(DockBaseConfig config) { }

        public static double GetSplitterWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(SplitterWidthProperty);
        }
        public static void SetSplitterWidth(DependencyObject obj, double value)
        {
            obj.SetValue(SplitterWidthProperty, value);
        }
        public static readonly DependencyProperty SplitterWidthProperty = DependencyProperty.RegisterAttached(
            "SplitterWidth", typeof(double), typeof(DockBayBase), new FrameworkPropertyMetadata(5.0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));
        public static System.Windows.Media.Brush GetSplitterBrush(DependencyObject obj)
        {
            return (System.Windows.Media.Brush)obj.GetValue(SplitterBrushProperty);
        }
        public static void SetSplitterBrush(DependencyObject obj, System.Windows.Media.Brush value)
        {
            obj.SetValue(SplitterBrushProperty, value);
        }
        public static readonly DependencyProperty SplitterBrushProperty = DependencyProperty.RegisterAttached(
            "SplitterBrush", typeof(System.Windows.Media.Brush), typeof(DockBayBase), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));
    }
    [Serializable]
    public class DockBayBaseConfig : DockBaseConfig
    {
        public int ChildCount { get; set; }
    }
}
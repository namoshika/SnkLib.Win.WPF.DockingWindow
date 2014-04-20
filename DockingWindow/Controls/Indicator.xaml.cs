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

namespace SunokoLibrary.Windows.Controls
{
    /// <summary>
    /// Indicator.xaml の相互作用ロジック
    /// </summary>
    public partial class Indicator : UserControl
    {
        public Indicator()
        {
            InitializeComponent();
            IsActive = false;
            //picActive.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            //picActive.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            //picNormal.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            //picNormal.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
        }
        private bool _isActive;

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public ImageSource ActiveImage
        {
            get { return (ImageSource)GetValue(ActiveImageProperty); }
            set { SetValue(ActiveImageProperty, value); }
        }
        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(Indicator), new PropertyMetadata((sender, e) =>
                {
                    var obj = sender as Indicator;
                    if ((obj.NormalImage != null) && (obj.ActiveImage != null))
                    {
                        obj.picActive.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;
                        obj.picNormal.Visibility = !(bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;
                        obj._isActive = (bool)e.NewValue;
                    }
                }));
        public static readonly DependencyProperty ActiveImageProperty = DependencyProperty.Register(
            "ActiveImage", typeof(ImageSource), typeof(Indicator),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
                (sender, e) =>
                {
                    var obj = (Indicator)sender;
                    obj.picActive.Source = (ImageSource)e.NewValue;
                }));
        public static readonly DependencyProperty NormalImageProperty = DependencyProperty.Register(
            "NormalImage", typeof(ImageSource), typeof(Indicator),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
                (sender, e) =>
                {
                    var obj = (Indicator)sender;
                    obj.picNormal.Source = (ImageSource)e.NewValue;
                }));
    }
}

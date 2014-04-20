using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Windows.Controls.Primitives
{
    public class SplitterWidthAndAlignToSizeConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            var align = (DockDirection)values.First(obj => obj is DockDirection);
            var width = (double)values.First(obj => obj is double);
            return parameter.ToString() == "height" ^ (align == DockDirection.Left || align == DockDirection.Right) ? width : double.NaN;
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, System.Globalization.CultureInfo culture)
        { throw new NotSupportedException(); }
    }
}
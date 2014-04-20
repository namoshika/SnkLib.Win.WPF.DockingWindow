using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace SunokoLibrary.Windows.Controls.Primitives
{
    public class DockDirectionToDock : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DockDirection)
                switch ((DockDirection)value)
                {
                    case DockDirection.Top:
                        return Dock.Bottom;
                    case DockDirection.Bottom:
                        return Dock.Top;
                    case DockDirection.Left:
                        return Dock.Right;
                    case DockDirection.Right:
                        return Dock.Left;
                    default:
                        return null;
                }
            else
                return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Dock)
                switch ((Dock)value)
                {
                    case Dock.Top:
                        return DockDirection.Top;
                    case Dock.Bottom:
                        return DockDirection.Bottom;
                    case Dock.Left:
                        return DockDirection.Left;
                    case Dock.Right:
                        return DockDirection.Right;
                    default:
                        return null;
                }
            else
                return null;
        }
    }
}
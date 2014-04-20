using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SunokoLibrary.Windows.Controls
{
    public static class DockCommands
    {
        static DockCommands()
        {
            CloseDockPane = new RoutedCommand("CloseDockPane", typeof(DockCommands));
            CloseFloatForm = new RoutedCommand("CloseFloatForm", typeof(DockCommands));
            HideDockPane = new RoutedCommand("HideDockPane", typeof(DockCommands));
            PurgeAndAttachMouse = new RoutedCommand("PurgeAndAttachMouse", typeof(DockCommands));
        }
        public static RoutedCommand CloseDockPane { get; private set; }
        public static RoutedCommand CloseFloatForm { get; private set; }
        public static RoutedCommand HideDockPane { get; private set; }
        public static RoutedCommand PurgeAndAttachMouse { get; private set; }
    }
}
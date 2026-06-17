using System.Windows;

namespace Konditerka.AppData
{
    /// <summary>
    /// Вызывай SetMinSize() в конструкторе каждой Page после InitializeComponent()
    /// </summary>
    public static class WindowSizeHelper
    {
        public static void SetMinSize(double minWidth, double minHeight)
        {
            if (Application.Current?.MainWindow == null) return;

            var win = Application.Current.MainWindow;
            win.MinWidth = minWidth;
            win.MinHeight = minHeight;
            if (win.Width < minWidth) win.Width = minWidth;
            if (win.Height < minHeight) win.Height = minHeight;
        }
    }
}
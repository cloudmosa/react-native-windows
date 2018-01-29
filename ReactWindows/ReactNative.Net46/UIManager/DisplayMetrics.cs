using System;
using System.Collections.Generic;
using System.Windows;

namespace ReactNative.UIManager
{
    class DisplayMetrics
    {
        private DisplayMetrics(double width, double height, double scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
        }

        public double Width { get; }

        public double Height { get; }

        public double Scale { get; }

        public static DisplayMetrics GetForCurrentView()
        {
            var window = Application.Current.MainWindow;
            var content = (FrameworkElement)window.Content;
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            double scale = 1.0;
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(hwnd))
            {
                scale = g.DpiX / 96;
            }
            return new DisplayMetrics(
                content?.ActualWidth ?? 0.0,
                content?.ActualHeight ?? 0.0,
                scale);
        }
    }
}

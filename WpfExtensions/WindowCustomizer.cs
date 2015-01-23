using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public static class WindowCustomizer
    {
        public static readonly DependencyProperty CanMaximize = DependencyProperty.RegisterAttached("CanMaximize", typeof(bool), typeof(Window), new PropertyMetadata((object)true, new PropertyChangedCallback(WindowCustomizer.OnCanMaximizeChanged)));
        public static readonly DependencyProperty CanMinimize = DependencyProperty.RegisterAttached("CanMinimize", typeof(bool), typeof(Window), new PropertyMetadata((object)true, new PropertyChangedCallback(WindowCustomizer.OnCanMinimizeChanged)));

        private static void OnCanMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = d as Window;
            if (window == null)
                return;
            RoutedEventHandler loadedHandler = (RoutedEventHandler)null;
            loadedHandler = (RoutedEventHandler)((param0, param1) =>
            {
                if ((bool)e.NewValue)
                    WindowCustomizer.WindowHelper.EnableMaximize(window);
                else
                    WindowCustomizer.WindowHelper.DisableMaximize(window);
                window.Loaded -= loadedHandler;
            });
            if (!window.IsLoaded)
                window.Loaded += loadedHandler;
            else
                loadedHandler((object)null, (RoutedEventArgs)null);
        }

        public static void SetCanMaximize(DependencyObject d, bool value)
        {
            d.SetValue(WindowCustomizer.CanMaximize, value);
        }

        public static bool GetCanMaximize(DependencyObject d)
        {
            return (bool)d.GetValue(WindowCustomizer.CanMaximize);
        }

        private static void OnCanMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = d as Window;
            if (window == null)
                return;
            RoutedEventHandler loadedHandler = (RoutedEventHandler)null;
            loadedHandler = (RoutedEventHandler)((param0, param1) =>
            {
                if ((bool)e.NewValue)
                    WindowCustomizer.WindowHelper.EnableMinimize(window);
                else
                    WindowCustomizer.WindowHelper.DisableMinimize(window);
                window.Loaded -= loadedHandler;
            });
            if (!window.IsLoaded)
                window.Loaded += loadedHandler;
            else
                loadedHandler((object)null, (RoutedEventArgs)null);
        }

        public static void SetCanMinimize(DependencyObject d, bool value)
        {
            d.SetValue(WindowCustomizer.CanMinimize, value);
        }

        public static bool GetCanMinimize(DependencyObject d)
        {
            return (bool)d.GetValue(WindowCustomizer.CanMinimize);
        }

        public static class WindowHelper
        {
            private const int GWL_STYLE = -16;
            private const int WS_MAXIMIZEBOX = 65536;
            private const int WS_MINIMIZEBOX = 131072;

            [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
            private static extern int GetWindowLongPtr(IntPtr hWnd, int nIndex);

            [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
            private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);

            public static void DisableMaximize(Window window)
            {
                lock (window)
                {
                    IntPtr local_0 = new WindowInteropHelper(window).Handle;
                    int local_1 = WindowCustomizer.WindowHelper.GetWindowLongPtr(local_0, -16);
                    WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 & -65537);
                }
            }

            public static void DisableMinimize(Window window)
            {
                lock (window)
                {
                    IntPtr local_0 = new WindowInteropHelper(window).Handle;
                    int local_1 = WindowCustomizer.WindowHelper.GetWindowLongPtr(local_0, -16);
                    WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 & -131073);
                }
            }

            public static void EnableMaximize(Window window)
            {
                lock (window)
                {
                    IntPtr local_0 = new WindowInteropHelper(window).Handle;
                    int local_1 = WindowCustomizer.WindowHelper.GetWindowLongPtr(local_0, -16);
                    WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 | 65536);
                }
            }

            public static void EnableMinimize(Window window)
            {
                lock (window)
                {
                    IntPtr local_0 = new WindowInteropHelper(window).Handle;
                    int local_1 = WindowCustomizer.WindowHelper.GetWindowLongPtr(local_0, -16);
                    WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 | 131072);
                }
            }

            public static void ToggleMaximize(Window window)
            {
                lock (window)
                {
                    IntPtr local_0 = new WindowInteropHelper(window).Handle;
                    int local_1 = WindowCustomizer.WindowHelper.GetWindowLongPtr(local_0, -16);
                    if ((local_1 | 65536) == local_1)
                        WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 & -65537);
                    else
                        WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 | 65536);
                }
            }

            public static void ToggleMinimize(Window window)
            {
                lock (window)
                {
                    IntPtr local_0 = new WindowInteropHelper(window).Handle;
                    int local_1 = WindowCustomizer.WindowHelper.GetWindowLongPtr(local_0, -16);
                    if ((local_1 | 131072) == local_1)
                        WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 & -131073);
                    else
                        WindowCustomizer.WindowHelper.SetWindowLongPtr(local_0, -16, local_1 | 131072);
                }
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public static class WindowsDisplayProperties
    {
        private const int LOGPIXELSX = 88;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public static double GetDpi()
        {
            return (double)WindowsDisplayProperties.GetDeviceCaps(WindowsDisplayProperties.GetDC(IntPtr.Zero), 88);
        }
    }
}

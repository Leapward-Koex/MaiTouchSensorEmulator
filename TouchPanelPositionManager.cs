using System.Runtime.InteropServices;
using System.Windows;

namespace WpfMaiTouchEmulator;

class TouchPanelPositionManager
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public Rect? GetSinMaiWindowPosition()
    {
        var hWnd = FindWindow(null, "Sinmai");
        if (hWnd != IntPtr.Zero)
        {
            RECT rect;
            if (GetWindowRect(hWnd, out rect))
            {
                // Calculate the desired size and position based on the other application's window
                var width = Convert.ToInt32((rect.Right - rect.Left));
                var height = width;
                var left = rect.Left + ((rect.Right - rect.Left) - width) / 2; // Center horizontally
                var top = rect.Bottom - height;
                return new Rect(left, top, width, height);
            }
        }
        return null;
    }
}

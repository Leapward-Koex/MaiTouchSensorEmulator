using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMaiTouchEmulator
{
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
            // Replace "OtherAppWindowName" with the window name (title) of the other application
            IntPtr hWnd = FindWindow(null, "Sinmai");
            if (hWnd != IntPtr.Zero)
            {
                RECT rect;
                if (GetWindowRect(hWnd, out rect))
                {
                    // Calculate the desired size and position based on the other application's window
                    int width = Convert.ToInt32((rect.Right - rect.Left));
                    int height = width;
                    int left = rect.Left + ((rect.Right - rect.Left) - width) / 2; // Center horizontally
                    int top = rect.Bottom - height;
                    return new Rect(left, top, width, height);
                }
            }
            return null;
        }
    }
}

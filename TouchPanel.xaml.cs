using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Imaging; // For BitmapImage
using System.IO;
using System.Windows.Interop; // For Imaging.CreateBitmapSourceFromHBitmap
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls.Primitives; // For Marshal

namespace WpfMaiTouchEmulator;
/// <summary>
/// Interaction logic for TouchPanel.xaml
/// </summary>
public partial class TouchPanel : Window
{
    internal Action<TouchValue> onTouch;
    internal Action<TouchValue> onRelease;
    private bool isResizing = false;
    private System.Windows.Point clickPosition;

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr window, int index, int value);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr window, int index);

    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int WS_EX_LAYERED = 0x00080000;

    public TouchPanel()
    {
        InitializeComponent();
        this.Topmost = true;
        var positionManager = new TouchPanelPositionManager();
        var position = positionManager.GetTouchPanelPosition();
        if (position != null )
        {
            Top = position.Value.Top;
            Left = position.Value.Left;
            Width = position.Value.Width;
            Height = position.Value.Height;
        }

        this.Loaded += new RoutedEventHandler(Window_Loaded);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Make the window transparent to input
        var hwnd = new WindowInteropHelper(this).Handle;
        //SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // This allows the entire window to be draggable
        DragMove();
    }

    private void DragBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // This event is for the draggable bar, it calls DragMove to move the window
        DragMove();
    }

    private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            ResizeWindow(ResizeDirection.BottomRight);
        }
    }

    private enum ResizeDirection
    {
        BottomRight = 8,
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private void ResizeWindow(ResizeDirection direction)
    {
        ReleaseCapture();
        SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle,
                    0x112, // WM_SYSCOMMAND message
                    (IntPtr)(0xF000 + direction),
                    IntPtr.Zero);
    }

    private Dictionary<int, System.Windows.Controls.Image> activeTouches = new Dictionary<int, System.Windows.Controls.Image>();


    private void Element_TouchDown(object sender, TouchEventArgs e)
    {
        // Cast the sender to a Border to ensure it's the correct element type.
        var element = sender as System.Windows.Controls.Image;
        if (element != null)
        {
            // Highlight the element and add it to the active touches tracking.
            HighlightElement(element, true);
            onTouch((TouchValue)element.Tag);
            activeTouches[e.TouchDevice.Id] = element;
        }
        e.Handled = true;
    }

    private void Element_TouchMove(object sender, TouchEventArgs e)
    {
        // Attempt to find the element under the current touch point.
        var touchPoint = e.GetTouchPoint(this).Position;
        var hitTestResult = VisualTreeHelper.HitTest(this, touchPoint);
        if (hitTestResult != null && hitTestResult.VisualHit is System.Windows.Controls.Image newElement)
        {
            // If this touch point is already tracking another element, unhighlight the previous one.
            if (activeTouches.TryGetValue(e.TouchDevice.Id, out System.Windows.Controls.Image previousElement) && previousElement != newElement)
            {
                HighlightElement(previousElement, false);
                onRelease((TouchValue)previousElement.Tag);
            }

            // Highlight the new element and update the tracking.
            HighlightElement(newElement, true);
            onTouch((TouchValue)newElement.Tag);
            activeTouches[e.TouchDevice.Id] = newElement;
        }

        if (!IsTouchInsideWindow(touchPoint))
        {
            // Touch is outside the window, act accordingly
            DeselectAllItems();
        }

        e.Handled = true;
    }

    private void Element_TouchUp(object sender, TouchEventArgs e)
    {
        // When touch is lifted, unhighlight the associated element and remove it from tracking.
        if (activeTouches.TryGetValue(e.TouchDevice.Id, out System.Windows.Controls.Image element))
        {
            HighlightElement(element, false);
            onRelease((TouchValue)element.Tag);
            activeTouches.Remove(e.TouchDevice.Id);
        }

        e.Handled = true;
    }

    private bool IsTouchInsideWindow(System.Windows.Point touchPoint)
    {
        // Define the window's bounds
        var windowBounds = new Rect(0, 0, this.ActualWidth, this.ActualHeight);

        // Check if the touch point is within the window's bounds
        return windowBounds.Contains(touchPoint);
    }

    private void DeselectAllItems()
    {
        // Logic to deselect all items or the last touched item
        foreach (var element in activeTouches.Values)
        {
            HighlightElement(element, false);
        }
        activeTouches.Clear();
    }

    public void SetDebugMode(bool enabled)
    {
        var buttons = VisualTreeHelperExtensions.FindVisualChildren<System.Windows.Controls.Image>(this);
        buttons.ForEach(button =>
        {
            button.Opacity = enabled ? 0.3 : 0;
        });
    }

    private void HighlightElement(System.Windows.Controls.Image element, bool highlight)
    {
        element.Opacity = highlight ? 0.8 : 0.3;
    }

}

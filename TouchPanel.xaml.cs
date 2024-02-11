using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfMaiTouchEmulator;
/// <summary>
/// Interaction logic for TouchPanel.xaml
/// </summary>
public partial class TouchPanel : Window
{
    internal Action<TouchValue> onTouch;
    internal Action<TouchValue> onRelease;

    private readonly Dictionary<int, System.Windows.Controls.Image> activeTouches = [];
    private readonly TouchPanelPositionManager _positionManager;
    private List<Image> buttons = [];

    private enum ResizeDirection
    {
        BottomRight = 8,
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public TouchPanel()
    {
        InitializeComponent();
        Topmost = true;
        _positionManager = new TouchPanelPositionManager();
        Loaded += Window_Loaded;

        StateCheckLoop();
    }

    private async void StateCheckLoop()
    {
        while (true)
        {
            if (activeTouches.Any() && !TouchesOver.Any())
            {
                await Task.Delay(100);
                if (activeTouches.Any() && !TouchesOver.Any())
                {
                    DeselectAllItems();
                }
            }
            await Task.Delay(100);
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        buttons = VisualTreeHelperExtensions.FindVisualChildren<System.Windows.Controls.Image>(this);
    }

    public void PositionTouchPanel()
    {
        var position = _positionManager.GetSinMaiWindowPosition();
        if (position != null)
        {
            Top = position.Value.Top;
            Left = position.Value.Left;
            Width = position.Value.Width;
            Height = position.Value.Height;
        }
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

    private void ResizeWindow(ResizeDirection direction)
    {
        ReleaseCapture();
        SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle,
                    0x112, // WM_SYSCOMMAND message
                    (IntPtr)(0xF000 + direction),
                    IntPtr.Zero);
    }

    private void Element_TouchDown(object sender, TouchEventArgs e)
    {
        // Cast the sender to a Border to ensure it's the correct element type.
        if (sender is System.Windows.Controls.Image element)
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
            if (activeTouches.TryGetValue(e.TouchDevice.Id, out var previousElement) && previousElement != newElement)
            {
                Task.Delay(50)
                    .ContinueWith(t =>
                    {
                        HighlightElement(previousElement, false);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            onRelease((TouchValue)previousElement.Tag);
                        });
                    });

            }

            // Highlight the new element and update the tracking.
            HighlightElement(newElement, true);
            onTouch((TouchValue)newElement.Tag);
            activeTouches[e.TouchDevice.Id] = newElement;
        }

        e.Handled = true;
    }

    private void Element_TouchUp(object sender, TouchEventArgs e)
    {
        // When touch is lifted, unhighlight the associated element and remove it from tracking.
        if (activeTouches.TryGetValue(e.TouchDevice.Id, out var element))
        {
            HighlightElement(element, false);
            onRelease((TouchValue)element.Tag);
            activeTouches.Remove(e.TouchDevice.Id);
        }

        e.Handled = true;
    }

    private bool IsTouchInsideWindow(Point touchPoint)
    {
        // Define the window's bounds
        var windowBounds = new Rect(0, 0, ActualWidth, ActualHeight);

        // Check if the touch point is within the window's bounds
        return windowBounds.Contains(touchPoint);
    }

    private void DeselectAllItems()
    {
        // Logic to deselect all items or the last touched item
        foreach (var element in activeTouches.Values)
        {
            HighlightElement(element, false);
            onRelease((TouchValue)element.Tag);
        }
        activeTouches.Clear();
    }

    public void SetDebugMode(bool enabled)
    {
        buttons.ForEach(button =>
        {
            button.Opacity = enabled ? 0.3 : 0;
        });
    }

    private void HighlightElement(System.Windows.Controls.Image element, bool highlight)
    {
        if (Properties.Settings.Default.IsDebugEnabled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                element.Opacity = highlight ? 0.8 : 0.3;
            });
        }
    }
}

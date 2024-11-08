using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfMaiTouchEmulator;
/// <summary>
/// Interaction logic for TouchPanel.xaml
/// </summary>
public partial class TouchPanel : Window
{
    internal Action<TouchValue>? onTouch;
    internal Action<TouchValue>? onRelease;
    internal Action? onInitialReposition;

    private readonly Dictionary<int, Polygon> activeTouches = [];
    private readonly TouchPanelPositionManager _positionManager;
    private List<Polygon> buttons = [];
    private bool isDebugEnabled = Properties.Settings.Default.IsDebugEnabled;
    private bool isRingButtonEmulationEnabled = Properties.Settings.Default.IsRingButtonEmulationEnabled;
    private bool hasRepositioned = false;

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
            if (activeTouches.Count != 0 && !TouchesOver.Any())
            {
                await Task.Delay(100);
                if (activeTouches.Count != 0 && !TouchesOver.Any())
                {
                    DeselectAllItems();
                }
            }
            await Task.Delay(100);
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        buttons = VisualTreeHelperExtensions.FindVisualChildren<Polygon>(this);
        DeselectAllItems();
    }

    public void PositionTouchPanel()
    {
        var position = _positionManager.GetSinMaiWindowPosition();
        if (position != null &&
            (Top != position.Value.Top || Left != position.Value.Left || Width != position.Value.Width || Height != position.Value.Height)
            )
        {
            Logger.Info("Touch panel not over sinmai window, repositioning");
            Top = position.Value.Top;
            Left = position.Value.Left;
            Width = position.Value.Width;
            Height = position.Value.Height;

            if (!hasRepositioned)
            {
                hasRepositioned = true;
                onInitialReposition?.Invoke();
            }
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
        if (sender is Polygon element)
        {
            // Highlight the element and add it to the active touches tracking.
            HighlightElement(element, true);
            var touchValue = (TouchValue)element.Tag;
            if (isRingButtonEmulationEnabled && RingButtonEmulator.HasRingButtonMapping((TouchValue)element.Tag))
            {
                RingButtonEmulator.PressButton((TouchValue)element.Tag);
            }
            else
            {
                onTouch?.Invoke((TouchValue)element.Tag);
            }
            activeTouches[e.TouchDevice.Id] = element;
        }
        e.Handled = true;
    }

    private void Element_TouchMove(object sender, TouchEventArgs e)
    {
        // Attempt to find the element under the current touch point.
        var touchPoint = e.GetTouchPoint(this).Position;
        var hitTestResult = VisualTreeHelper.HitTest(this, touchPoint);
        if (hitTestResult != null && hitTestResult.VisualHit is Polygon newElement)
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
                            onRelease?.Invoke((TouchValue)previousElement.Tag);
                        });
                    });

            }

            // Highlight the new element and update the tracking.
            HighlightElement(newElement, true);
            onTouch?.Invoke((TouchValue)newElement.Tag);
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
            RingButtonEmulator.ReleaseButton((TouchValue)element.Tag);
            onRelease?.Invoke((TouchValue)element.Tag);
            activeTouches.Remove(e.TouchDevice.Id);
        }

        e.Handled = true;
    }

    private void DeselectAllItems()
    {
        // Logic to deselect all items or the last touched item
        foreach (var element in activeTouches.Values)
        {
            HighlightElement(element, false);
            onRelease?.Invoke((TouchValue)element.Tag);
        }
        activeTouches.Clear();
        RingButtonEmulator.ReleaseAllButtons();
    }

    public void SetDebugMode(bool enabled)
    {
        isDebugEnabled = enabled;
        buttons.ForEach(button =>
        {
            button.Opacity = enabled ? 0.3 : 0;
        });
    }

    public void SetEmulateRingButton(bool enabled)
    {
        isRingButtonEmulationEnabled = enabled;
    }

    private void HighlightElement(Polygon element, bool highlight)
    {
        if (isDebugEnabled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                element.Opacity = highlight ? 0.8 : 0.3;
            });
        }
    }
}

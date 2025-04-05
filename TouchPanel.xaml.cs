using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfMaiTouchEmulator.Managers;

namespace WpfMaiTouchEmulator;
/// <summary>
/// Interaction logic for TouchPanel.xaml
/// </summary>
public partial class TouchPanel : Window
{
    internal Action<TouchValue>? onTouch;
    internal Action<TouchValue>? onRelease;
    internal Action? onInitialReposition;

    private readonly Dictionary<int, (Polygon polygon, Point lastPoint)> activeTouches = new Dictionary<int, (Polygon, Point)>();
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
        Touch.FrameReported += OnTouchFrameReported;
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

    private void OnTouchFrameReported(object sender, TouchFrameEventArgs e)
    {
        var currentTouchPoints = e.GetTouchPoints(this);
        var currentIds = new HashSet<int>();

        foreach (var touch in currentTouchPoints)
        {
            var id = touch.TouchDevice.Id;

            // If the touch is released, process it as a TouchUp.
            if (touch.Action == TouchAction.Up)
            {
                if (activeTouches.TryGetValue(id, out var touchInfo2))
                {
                    if (activeTouches.Values.Count(v => v.polygon == touchInfo2.polygon) == 1)
                    {
                        HighlightElement(touchInfo2.polygon, false);
                        onRelease?.Invoke((TouchValue)touchInfo2.polygon.Tag);
                        if (isRingButtonEmulationEnabled)
                        {
                            RingButtonEmulator.ReleaseButton((TouchValue)touchInfo2.polygon.Tag);
                        }
                    }
                    activeTouches.Remove(id);
                }
                continue;
            }

            currentIds.Add(id);

            // New touch (TouchDown)
            if (!activeTouches.TryGetValue(id, out var touchInfo))
            {
                if (VisualTreeHelper.HitTest(this, touch.Position)?.VisualHit is Polygon polygon)
                {
                    HighlightElement(polygon, true);
                    activeTouches[id] = (polygon, touch.Position);
                    onTouch?.Invoke((TouchValue)polygon.Tag);
                    if (isRingButtonEmulationEnabled && RingButtonEmulator.HasRingButtonMapping((TouchValue)polygon.Tag))
                    {
                        RingButtonEmulator.PressButton((TouchValue)polygon.Tag);
                    }
                }
            }
            // Existing touch (TouchMove)
            else
            {
                var previousPosition = touchInfo.lastPoint;
                var currentPosition = touch.Position;
                var sampleCount = 10;
                var changed = false;

                for (var i = 1; i <= sampleCount; i++)
                {
                    var t = (double)i / sampleCount;
                    var samplePoint = new Point(
                        previousPosition.X + (currentPosition.X - previousPosition.X) * t,
                        previousPosition.Y + (currentPosition.Y - previousPosition.Y) * t);
                    if (VisualTreeHelper.HitTest(this, samplePoint)?.VisualHit is Polygon polygon && polygon != touchInfo.polygon)
                    {
                        if (activeTouches.Values.Count(v => v.polygon == touchInfo.polygon) == 1)
                        {
                            HighlightElement(touchInfo.polygon, false);
                            onRelease?.Invoke((TouchValue)touchInfo.polygon.Tag);
                        }
                        HighlightElement(polygon, true);
                        onTouch?.Invoke((TouchValue)polygon.Tag);
                        activeTouches[id] = (polygon, samplePoint);
                        changed = true;
                        break;
                    }
                }
                if (!changed)
                {
                    activeTouches[id] = (touchInfo.polygon, currentPosition);
                }
            }
        }

        // Process any touches that might not be reported this frame.
        var endedTouches = activeTouches.Keys.Except(currentIds).ToList();
        foreach (var id in endedTouches)
        {
            var touchInfo = activeTouches[id];
            if (activeTouches.Values.Count(v => v.polygon == touchInfo.polygon) == 1)
            {
                HighlightElement(touchInfo.polygon, false);
                onRelease?.Invoke((TouchValue)touchInfo.polygon.Tag);
                if (isRingButtonEmulationEnabled)
                {
                    RingButtonEmulator.ReleaseButton((TouchValue)touchInfo.polygon.Tag);
                }
            }
            activeTouches.Remove(id);
        }
    }

    private void DeselectAllItems()
    {
        // Logic to deselect all items or the last touched item
        foreach (var element in activeTouches.Values)
        {
            HighlightElement(element.polygon, false);
            onRelease?.Invoke((TouchValue)element.polygon.Tag);
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

    public void SetLargeButtonMode(bool enabled)
    {
        TouchValue[] ringButtonsValues = {
            TouchValue.A1,
            TouchValue.A2,
            TouchValue.A3,
            TouchValue.A4,
            TouchValue.A5,
            TouchValue.A6,
            TouchValue.A7,
            TouchValue.A8,
            TouchValue.D1,
            TouchValue.D2,
            TouchValue.D3,
            TouchValue.D4,
            TouchValue.D5,
            TouchValue.D6,
            TouchValue.D7,
            TouchValue.D8,
        };

        var a1 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A1);
        var a2 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A2);
        var a3 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A3);
        var a4 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A4);
        var a5 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A5);
        var a6 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A6);
        var a7 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A7);
        var a8 = buttons.First(button => (TouchValue)button.Tag == TouchValue.A8);
        var d1 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D1);
        var d2 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D2);
        var d3 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D3);
        var d4 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D4);
        var d5 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D5);
        var d6 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D6);
        var d7 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D7);
        var d8 = buttons.First(button => (TouchValue)button.Tag == TouchValue.D8);

        if (enabled)
        {
            d1.Points = new PointCollection
            {
                new Point(-5, -50),
                new Point(205, -50),
                new Point(165, 253),
                new Point(100, 188),
                new Point(35, 253),
            };

            a1.Points = new PointCollection
            {
                new Point(495, -50),
                new Point(208, 338),
                new Point(145, 338),
                new Point(49, 297),
                new Point(0, 249),
                new Point(42, -55),
            };
            d2.Points = new PointCollection
            {
                new Point(290, -182),
                new Point(500, -180),
                new Point(500, -5),
                new Point(96, 297),
                new Point(96, 205),
                new Point(0, 205),
            };
            a2.Points = new PointCollection
            {
                new Point(405, 317),
                new Point(91, 362),
                new Point(42, 314),
                new Point(0, 219),
                new Point(0, 150),
                new Point(405, -150),
            };
            d3.Points = new PointCollection
            {
                new Point(315, -10),
                new Point(315, 208),
                new Point(0, 165),
                new Point(65, 100),
                new Point(0, 35),
            };
            a3.Points = new PointCollection
            {
                new Point(406, 520),
                new Point(0, 213),
                new Point(0, 144),
                new Point(41, 48),
                new Point(89, 0),
                new Point(406, 43),
            };
            d4.Points = new PointCollection
            {
                new Point(500, 309),
                new Point(500, 491),
                new Point(305, 491),
                new Point(0, 92),
                new Point(92, 92),
                new Point(92, 0),
            };
            a4.Points = new PointCollection
            {
                new Point(45, 400),
                new Point(0, 83),
                new Point(48, 35),
                new Point(144, 0),
                new Point(212, 0),
                new Point(515, 400),
            };
            d5.Points = new PointCollection
            {
                new Point(208, 317),
                new Point(-10, 317),
                new Point(34, 0),
                new Point(99, 65),
                new Point(164, 0),
            };

            a5.Points = new PointCollection
            {
                new Point(317, 400),
                new Point(363, 83),
                new Point(316, 35),
                new Point(220, 0),
                new Point(152, 0),
                new Point(-150, 400),
            };
            d6.Points = new PointCollection
            {
                new Point(-10, 492),
                new Point(-200, 492),
                new Point(-200, 295),
                new Point(199, 0),
                new Point(199, 92),
                new Point(291, 92),
            };
            a6.Points = new PointCollection
            {
                new Point(-67, 505),
                new Point(333, 214),
                new Point(333, 144),
                new Point(296, 48),
                new Point(248, 0),
                new Point(-67, 45),
            };

            d7.Points = new PointCollection
            {
                new Point(-60, 207),
                new Point(-60, -7),
                new Point(253, 34),
                new Point(188, 99),
                new Point(253, 164),
            };

            a7.Points = new PointCollection
            {
                new Point(-65, 320),
                new Point(248, 362),
                new Point(297, 314),
                new Point(333, 219),
                new Point(333, 151),
                new Point(-65, -150),
            };
            d8.Points = new PointCollection
            {
                new Point(-195, -10),
                new Point(-195, -195),
                new Point(-5, -195),
                new Point(298, 199),
                new Point(200, 199),
                new Point(200, 291),
            };

            a8.Points = new PointCollection
            {
                new Point(-148, -55),
                new Point(153, 338),
                new Point(215, 338),
                new Point(311, 297),
                new Point(359, 249),
                new Point(318, -55),
            };
        }
        else
        {
            d1.Points = new PointCollection
            {
                new Point(0, 5),
                new Point(50, 2),
                new Point(100, 0),
                new Point(150, 2),
                new Point(200, 5),
                new Point(165, 253),
                new Point(100, 188),
                new Point(35, 253),
            };

            a1.Points = new PointCollection
            {
                new Point(150, 28),
                new Point(245, 65),
                new Point(360, 133),
                new Point(208, 338),
                new Point(145, 338),
                new Point(49, 297),
                new Point(0, 249),
                new Point(35, 0),
            };

            d2.Points = new PointCollection
            {
                new Point(153, 0),
                new Point(187, 32),
                new Point(225, 67),
                new Point(259, 104),
                new Point(295, 147),
                new Point(96, 297),
                new Point(96, 205),
                new Point(0, 205),
            };

            a2.Points = new PointCollection
            {
                new Point(261, 101),
                new Point(303, 195),
                new Point(339, 327),
                new Point(91, 362),
                new Point(42, 314),
                new Point(0, 219),
                new Point(0, 150),
                new Point(202, 0),
            };

            d3.Points = new PointCollection
            {
                new Point(248, 0),
                new Point(251, 48),
                new Point(253, 100),
                new Point(251, 150),
                new Point(247, 199),
                new Point(0, 165),
                new Point(65, 100),
                new Point(0, 35),
            };

            a3.Points = new PointCollection
            {
                new Point(305, 150),
                new Point(269, 246),
                new Point(201, 364),
                new Point(0, 213),
                new Point(0, 144),
                new Point(41, 48),
                new Point(89, 0),
                new Point(337, 34),
            };

            d4.Points = new PointCollection
            {
                new Point(292, 151),
                new Point(260, 187),
                new Point(225, 225),
                new Point(188, 259),
                new Point(151, 291),
                new Point(0, 92),
                new Point(92, 92),
                new Point(92, 0),
            };

            a4.Points = new PointCollection
            {
                new Point(260, 259),
                new Point(167, 301),
                new Point(37, 335),
                new Point(0, 83),
                new Point(48, 35),
                new Point(144, 0),
                new Point(212, 0),
                new Point(364, 200),
            };

            d5.Points = new PointCollection
            {
                new Point(199, 252),
                new Point(151, 255),
                new Point(99, 257),
                new Point(49, 255),
                new Point(0, 252),
                new Point(34, 0),
                new Point(99, 65),
                new Point(164, 0),
            };

            a5.Points = new PointCollection
            {
                new Point(104, 259),
                new Point(197, 301),
                new Point(327, 335),
                new Point(363, 83),
                new Point(316, 35),
                new Point(220, 0),
                new Point(152, 0),
                new Point(0, 201),
            };

            d6.Points = new PointCollection
            {
                new Point(140, 292),
                new Point(104, 260),
                new Point(66, 225),
                new Point(32, 188),
                new Point(0, 151),
                new Point(199, 0),
                new Point(199, 92),
                new Point(291, 92),
            };

            a6.Points = new PointCollection
            {
                new Point(32, 150),
                new Point(68, 246),
                new Point(133, 365),
                new Point(333, 214),
                new Point(333, 144),
                new Point(296, 48),
                new Point(248, 0),
                new Point(0, 35),
            };

            d7.Points = new PointCollection
            {
                new Point(5, 199),
                new Point(2, 151),
                new Point(0, 99),
                new Point(2, 49),
                new Point(6, 0),
                new Point(253, 34),
                new Point(188, 99),
                new Point(253, 164),
            };

            a7.Points = new PointCollection
            {
                new Point(78, 101),
                new Point(36, 195),
                new Point(0, 327),
                new Point(248, 362),
                new Point(297, 314),
                new Point(333, 219),
                new Point(333, 151),
                new Point(132, 0),
            };

            d8.Points = new PointCollection
            {
                new Point(0, 140),
                new Point(32, 104),
                new Point(67, 66),
                new Point(104, 32),
                new Point(145, 0),
                new Point(298, 199),
                new Point(200, 199),
                new Point(200, 291),
            };

            a8.Points = new PointCollection
            {
                new Point(210, 28),
                new Point(115, 65),
                new Point(0, 138),
                new Point(153, 338),
                new Point(215, 338),
                new Point(311, 297),
                new Point(359, 249),
                new Point(324, 0),
            };
        }
    }

    public void SetBorderMode(BorderSetting borderSetting, string borderColour)
    {
        if (borderSetting == BorderSetting.Rainbow)
        {
            var rotateTransform = new RotateTransform { CenterX = 0.5, CenterY = 0.5 };
            touchPanelBorder.BorderBrush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/Assets/conicalGradient.png")),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewport = new Rect(0, 0, 1, 1),
                TileMode = TileMode.Tile,
                RelativeTransform = rotateTransform,
            };

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(10)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
            return;
        }
        else if (borderSetting == BorderSetting.Solid)
        {
            try
            {
                var colour = (Color)ColorConverter.ConvertFromString(borderColour);
                touchPanelBorder.BorderBrush = new SolidColorBrush { Color = colour };
                return;

            }
            catch (Exception ex)
            {
                Logger.Error("Failed to parse solid colour", ex);
            }
        }
        touchPanelBorder.BorderBrush = null;
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

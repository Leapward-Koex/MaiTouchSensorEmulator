using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfMaiTouchEmulator;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MaiTouchSensorButtonStateManager buttonState;
    private MaiTouchComConnector connector;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Mai Touch Emulator";
        buttonState = new MaiTouchSensorButtonStateManager(buttonStateValue);
        connector = new MaiTouchComConnector(buttonState);
        connector.OnConnectStatusChange = (status) => Title = $"Mai Touch Emulator - {status}";
        connector.OnDataSent = (data) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SentLogBox.AppendText(data + Environment.NewLine);
                SentLogBox.ScrollToEnd();
            });
        };
        connector.OnDataRecieved = (data) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RecievedLogBox.AppendText(data + Environment.NewLine);
                RecievedLogBox.ScrollToEnd();
            });
        };

        var touchPanel = new TouchPanel();
        touchPanel.onTouch = (value) => { buttonState.PressButton(value); };
        touchPanel.onRelease = (value) => { buttonState.ReleaseButton(value); };
        touchPanel.Show();
        touchPanel.SetDebugMode(true);
    }

    private async void ConnectToPortButton_Click(object sender, RoutedEventArgs e)
    {
        await connector.startLoopAsync();
    }

    private void Touch_A1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        buttonState.PressButton(TouchValue.A1);
    }

    private void Touch_A1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        buttonState.ReleaseButton(TouchValue.A1);
    }

    private void Touch_A2_C1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        buttonState.PressButton(TouchValue.A2);
        buttonState.PressButton(TouchValue.C1);

    }

    private void Touch_A2_C1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        buttonState.ReleaseButton(TouchValue.A2);
        buttonState.ReleaseButton(TouchValue.C1);
    }
}
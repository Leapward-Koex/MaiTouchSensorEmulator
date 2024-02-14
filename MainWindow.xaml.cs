using System.Diagnostics;
using System.Windows;

namespace WpfMaiTouchEmulator;

public partial class MainWindow : Window
{
    private readonly MaiTouchSensorButtonStateManager buttonState;
    private MaiTouchComConnector connector;
    private readonly VirtualComPortManager comPortManager;
    private TouchPanel _touchPanel;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Mai Touch Emulator";
        buttonState = new MaiTouchSensorButtonStateManager(buttonStateValue);
        connector = new MaiTouchComConnector(buttonState);
        comPortManager = new VirtualComPortManager();
        connector.OnConnectStatusChange = (status) =>
        {
            connectionStateLabel.Content = status;
        };

        connector.OnConnectError = () =>
        {
            var dataContext = (MainWindowViewModel)DataContext;
            dataContext.IsAutomaticPortConnectingEnabled = false;
        };

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

        if (Properties.Settings.Default.FirstOpen)
        {
            Logger.Info("First open occurred");
            MessageBox.Show("Please remove any COM devices using the COM3 port before installing the virtual COM port. In Device Manager click \"View\" then enabled \"Show hidden devices\" and uninstall any devices that are using the COM3 port.\n\nAfter ensuring COM3 is free please use the install COM port button in the app to register the app.\n\nThe app needs to connect to the port prior to Sinmai.exe being opened.", "First time setup", MessageBoxButton.OK, MessageBoxImage.Information);
            Properties.Settings.Default.FirstOpen = false;
            Properties.Settings.Default.Save();
        }

        Loaded += (s, e) => {
            Logger.Info("Main window loaded, creating touch panel");
            _touchPanel = new TouchPanel();
            _touchPanel.onTouch = (value) => { buttonState.PressButton(value); };
            _touchPanel.onRelease = (value) => { buttonState.ReleaseButton(value); };
            _touchPanel.Show();
            _touchPanel.Owner = this;
            DataContext = new MainWindowViewModel()
            {
                IsDebugEnabled = Properties.Settings.Default.IsDebugEnabled,
                IsAutomaticPortConnectingEnabled = Properties.Settings.Default.IsAutomaticPortConnectingEnabled,
                IsAutomaticPositioningEnabled = Properties.Settings.Default.IsAutomaticPositioningEnabled,
                IsExitWithSinmaiEnabled = Properties.Settings.Default.IsExitWithSinmaiEnabled
            };

            var dataContext = (MainWindowViewModel)DataContext;
            _touchPanel.SetDebugMode(dataContext.IsDebugEnabled);
            AutomaticTouchPanelPositioningLoop();
            AutomaticPortConnectingLoop();
            ExitWithSinmaiLoop();
        };
    }

    private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        await connector.Disconnect();
        foreach (Window childWindow in OwnedWindows)
        {
            childWindow.Close();
        }
        Closing -= MainWindow_Closing;
        Close();
    }

    private async void ExitWithSinmaiLoop()
    {
        Process? sinamiProcess = null;
        while (sinamiProcess == null)
        {
            var processes = Process.GetProcessesByName("Sinmai");
            if (processes.Length > 0)
            {
                Logger.Info("Found sinmai process to exit alongside with");
                sinamiProcess = processes[0];
            }
            else
            {
                await Task.Delay(1000);
            }
        }
        var dataContext = (MainWindowViewModel)DataContext;

        if (dataContext.IsExitWithSinmaiEnabled)
        {
            try
            {
                await sinamiProcess.WaitForExitAsync();
                Logger.Info("Sinmai exited");
                if (dataContext.IsExitWithSinmaiEnabled)
                {
                    Logger.Info("Disconnecting from COM port before shutting down");
                    await connector.Disconnect();
                    Logger.Info("Shutting down...");
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to wait for sinmai to exit", ex);
                dataContext.IsExitWithSinmaiEnabled = false;
                Properties.Settings.Default.IsExitWithSinmaiEnabled = dataContext.IsExitWithSinmaiEnabled;
                Properties.Settings.Default.Save();
                MessageBox.Show("Failed to listen for Sinmai exit signal, is it running as admin?\n\nAutomatic exiting disabled.", "Failed to listen for Sinmai exit", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private async void AutomaticTouchPanelPositioningLoop()
    {
        var dataContext = (MainWindowViewModel)DataContext;
        while (true)
        {
            if (dataContext.IsAutomaticPositioningEnabled)
            {
                _touchPanel.PositionTouchPanel();
            }
        
            await Task.Delay(1000);
        }
    }

    private async void AutomaticPortConnectingLoop()
    {
        var dataContext = (MainWindowViewModel)DataContext;
        while (true)
        {
            if (dataContext.IsAutomaticPortConnectingEnabled)
            {
                await connector.StartTouchSensorPolling();
            }
            await Task.Delay(1000);
        }
    }
    

    private async void ConnectToPortButton_Click(object sender, RoutedEventArgs e)
    {
        await connector.StartTouchSensorPolling();
    }

    private void debugMode_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsDebugEnabled;
        dataContext.IsDebugEnabled = !enabled;
        Properties.Settings.Default.IsDebugEnabled = dataContext.IsDebugEnabled;
        Properties.Settings.Default.Save();
        _touchPanel.SetDebugMode(dataContext.IsDebugEnabled);
    }

    private void automaticTouchPanelPositioning_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsAutomaticPositioningEnabled;
        dataContext.IsAutomaticPositioningEnabled = !enabled;
        Properties.Settings.Default.IsAutomaticPositioningEnabled = dataContext.IsAutomaticPositioningEnabled;
        Properties.Settings.Default.Save();
    }

    private void automaticPortConnecting_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsAutomaticPortConnectingEnabled;
        dataContext.IsAutomaticPortConnectingEnabled = !enabled;
        Properties.Settings.Default.IsAutomaticPortConnectingEnabled = dataContext.IsAutomaticPortConnectingEnabled;
        Properties.Settings.Default.Save();
    }

    private void exitWithSinmai_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsExitWithSinmaiEnabled;
        dataContext.IsExitWithSinmaiEnabled = !enabled;
        Properties.Settings.Default.IsExitWithSinmaiEnabled = dataContext.IsExitWithSinmaiEnabled;
        Properties.Settings.Default.Save();
    }

    private async void buttonInstallComPort_Click(object sender, RoutedEventArgs e)
    {
        await comPortManager.InstallComPort();
    }

    private async void buttonUninstallComPorts_Click(object sender, RoutedEventArgs e)
    {
        await comPortManager.UninstallVirtualPorts();
    }

    private void buttonListComPorts_Click(object sender, RoutedEventArgs e)
    {
        var output = comPortManager.GetInstalledPorts();
        MessageBox.Show(string.Join("\n", output), "Installed ports");
    }
}

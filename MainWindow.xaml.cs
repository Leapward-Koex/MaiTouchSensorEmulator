﻿using System.Diagnostics;
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
    private readonly TouchPanel _touchPanel;

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

        _touchPanel = new TouchPanel();
        _touchPanel.onTouch = (value) => { buttonState.PressButton(value); };
        _touchPanel.onRelease = (value) => { buttonState.ReleaseButton(value); };
        _touchPanel.Show();
        _touchPanel.SetDebugMode(true);
        DataContext = new MainWindowViewModel() { IsDebugEnabled = true, IsAutomaticPortConnectingEnabled = true, IsAutomaticPositioningEnabled = true, IsExitWithSinmaiEnabled = true };
        AutomaticTouchPanelPositioningLoop();
        ExitWithSinmaiLoop();
    }

    private async void ExitWithSinmaiLoop()
    {
        Process? sinamiProcess = null;
        while (sinamiProcess == null)
        {
            var processes = Process.GetProcessesByName("Sinmai");
            if (processes.Length > 0)
            {
                sinamiProcess = processes[0];
            }
            else
            {
                await Task.Delay(1000);
            }
        }
        await sinamiProcess.WaitForExitAsync();
        var dataContext = (MainWindowViewModel)DataContext;
        if (dataContext.IsExitWithSinmaiEnabled)
        {
            Application.Current.Shutdown();
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

    private async void ConnectToPortButton_Click(object sender, RoutedEventArgs e)
    {
        await connector.startLoopAsync();
    }

    private void debugMode_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsDebugEnabled;
        dataContext.IsDebugEnabled = !enabled;
        _touchPanel.SetDebugMode(dataContext.IsDebugEnabled);
    }

    private void automaticTouchPanelPositioning_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsAutomaticPositioningEnabled;
        dataContext.IsAutomaticPositioningEnabled = !enabled;
    }

    private void automaticPortConnecting_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsAutomaticPortConnectingEnabled;
        dataContext.IsAutomaticPortConnectingEnabled = !enabled;
    }

    private void exitWithSinmai_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (MainWindowViewModel)DataContext;
        var enabled = !dataContext.IsExitWithSinmaiEnabled;
        dataContext.IsExitWithSinmaiEnabled = !enabled;
    }
}
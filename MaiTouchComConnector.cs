using System.IO.Ports;
using System.Windows;

namespace WpfMaiTouchEmulator;
internal class MaiTouchComConnector(MaiTouchSensorButtonStateManager buttonState)
{
    private static SerialPort? serialPort;
    private bool isActiveMode;
    private bool _connected;
    private bool _shouldReconnect = true;
    private readonly MaiTouchSensorButtonStateManager _buttonState = buttonState;

    public Action<string>? OnConnectStatusChange
    {
        get;
        internal set;
    }
    public Action? OnConnectError
    {
        get;
        internal set;
    }
    public Action<string>? OnDataSent
    {
        get;
        internal set;
    }
    public Action<string>? OnDataRecieved
    {
        get;
        internal set;
    }

    public async Task StartTouchSensorPolling()
    {
        if (!_connected && _shouldReconnect)
        {
            Logger.Info("Trying to connect to COM port...");
            var virtualPort = "COM23"; // Adjust as needed
            try
            {
                OnConnectStatusChange?.Invoke("Conecting...");
                serialPort = new SerialPort(virtualPort, 9600, Parity.None, 8, StopBits.One)
                {
                    WriteTimeout = 100
                };
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();
                Logger.Info("Serial port opened successfully.");
                OnConnectStatusChange?.Invoke("Connected to port");
                _connected = true;

                while (true)
                {
                    if (isActiveMode)
                    {
                        SendTouchscreenState();
                        await Task.Delay(1);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }

            }
            catch (TimeoutException) { }
            catch (Exception ex)
            {
                OnConnectError?.Invoke();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message, "Error connecting to COM port", MessageBoxButton.OK, MessageBoxImage.Error);
                });

            }
            finally
            {
                Logger.Info("Disconnecting from COM port");
                _connected = false;
                OnConnectStatusChange?.Invoke("Not Connected");
                if (serialPort?.IsOpen == true)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.Close();
                }
            }
        }
    }

    public async Task Disconnect()
    {
        Logger.Info("Disconnecting from COM port");
        _shouldReconnect = false;
        _connected = false;
        try
        {
            if (serialPort != null)
            {
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = false;
                serialPort.DataReceived -= SerialPort_DataReceived;
                await Task.Delay(200);
                if (serialPort.IsOpen == true)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.Close();
                }
            }

        }
        catch (Exception ex)
        {
            Logger.Error("Error whilst disconnecting from COM port", ex);
            MessageBox.Show(ex.Message);
        }
    }

    void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var recievedData = serialPort?.ReadExisting();
        var commands = recievedData?.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries);
        if (commands != null)
        {
            foreach (var command in commands)
            {
                var cleanedCommand = command.TrimStart('{');
                Logger.Info($"Received serial data: {cleanedCommand}");
                OnDataRecieved?.Invoke(cleanedCommand);

                if (cleanedCommand == "STAT")
                {
                    isActiveMode = true;
                }
                else if (cleanedCommand == "RSET")
                {

                }
                else if (cleanedCommand == "HALT")
                {
                    isActiveMode = false;
                }
                else if (cleanedCommand[2] == 'r' || cleanedCommand[2] == 'k')
                {
                    var leftOrRight = cleanedCommand[0];
                    var sensor = cleanedCommand[1];
                    var ratio = cleanedCommand[3];

                    var newString = $"({leftOrRight}{sensor}{cleanedCommand[2]}{ratio})";
                    serialPort?.Write(newString);
                    OnDataSent?.Invoke(newString);
                }
                else
                {
                    Logger.Warn($"Unhandled serial data command {cleanedCommand}");
                }
            }
        }
    }

    void SendTouchscreenState()
    {
        if (_connected)
        {
            var currentState = _buttonState.GetCurrentState();
            serialPort?.Write(currentState, 0, currentState.Length);
        }
    }
}

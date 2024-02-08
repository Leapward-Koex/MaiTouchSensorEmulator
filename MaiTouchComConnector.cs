using System.IO.Ports;
using System.Windows;

namespace WpfMaiTouchEmulator;
internal class MaiTouchComConnector
{
    private static SerialPort? serialPort;
    private bool isActiveMode;
    private bool _connected;
    private readonly MaiTouchSensorButtonStateManager _buttonState;

    public Action<string> OnConnectStatusChange
    {
        get;
        internal set;
    }
    public Action<string> OnDataSent
    {
        get;
        internal set;
    }
    public Action<string> OnDataRecieved
    {
        get;
        internal set;
    }

    public MaiTouchComConnector(MaiTouchSensorButtonStateManager buttonState)
    {
        _buttonState = buttonState;
    }

    public async Task StartTouchSensorPolling()
    {
        if (!_connected)
        {
            var virtualPort = "COM23"; // Adjust as needed
            try
            {
                OnConnectStatusChange("Conecting...");
                serialPort = new SerialPort(virtualPort, 9600, Parity.None, 8, StopBits.One);
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();
                Console.WriteLine("Serial port opened successfully.");
                OnConnectStatusChange("Connected to port");
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening serial port: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message, "Error connecting to COM port", MessageBoxButton.OK, MessageBoxImage.Error);
                });

            }
            finally
            {
                _connected = false;
                OnConnectStatusChange("Not Connected");
                if (serialPort?.IsOpen == true)
                {
                    serialPort.Close();
                }
            }
        }
    }

    void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var recievedData = serialPort.ReadExisting();
        var commands = recievedData.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var command in commands)
        {
            var cleanedCommand = command.TrimStart('{');
            Console.WriteLine($"Received data: {cleanedCommand}");
            OnDataRecieved(cleanedCommand);

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
                serialPort.Write(newString);
                OnDataSent(newString);
            }
            else
            {
                Console.WriteLine(cleanedCommand);
            }
        }
    }

    void SendTouchscreenState()
    {
        var currentState = _buttonState.GetCurrentState();
        serialPort?.Write(currentState, 0, currentState.Length);
    }
}

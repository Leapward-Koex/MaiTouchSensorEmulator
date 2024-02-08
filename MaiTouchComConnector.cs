using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMaiTouchEmulator;
internal class MaiTouchComConnector
{
    private static SerialPort? serialPort;
    private bool isActiveMode;
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

    public async Task startLoopAsync()
    {
        string virtualPort = "COM23"; // Adjust as needed


        try
        {

            // Use setupc.exe to create a virtual COM port pair
            //StartProcessWithAdminRights("C:\\Program Files (x86)\\com0com\\setupc.exe", $"PortName=COM3 PortName=COM23");

            serialPort = new SerialPort(virtualPort, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
            Console.WriteLine("Serial port opened successfully.");
            OnConnectStatusChange("Connected");



            // Simulate receiving a STAT packet
            // Keep the program running to simulate active mode
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
            OnConnectStatusChange("Not Connected");
            // Close the serial port when done
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }

            // Use setupc.exe to remove the virtual COM port pair with administrator privileges
            //StartProcessWithAdminRights("C:\\Program Files (x86)\\com0com\\setupc.exe", $"remove 0");
        }
    }

    void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string recievedData = serialPort.ReadExisting();
        var commands = recievedData.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string command in commands)
        {
            string cleanedCommand = command.TrimStart('{');
            // Implement your logic to process the received data here
            Console.WriteLine($"Received data: {cleanedCommand}");
            OnDataRecieved(cleanedCommand);


            // Check if the received packet is a STAT packet
            if (cleanedCommand == "STAT")
            {
                // Simulate entering active mode
                isActiveMode = true;
                Console.WriteLine("Entered Active Mode");
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
                char leftOrRight = cleanedCommand[0];
                char sensor = cleanedCommand[1];
                char ratio = cleanedCommand[3];

                // Create the new string in the specified format
                string newString = $"({leftOrRight}{sensor}{cleanedCommand[2]}{ratio})";
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
        //Console.WriteLine($"Sent Touchscreen State: {report}");*/
    }
}

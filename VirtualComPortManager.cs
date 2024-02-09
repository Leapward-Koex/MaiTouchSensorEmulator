using System.Diagnostics;
using System.IO.Ports;
using System.Windows;

namespace WpfMaiTouchEmulator;

internal class VirtualComPortManager
{
    public IEnumerable<string> GetInstalledPorts()
    {
        return SerialPort.GetPortNames();
    }

    public async Task<bool> CheckIfPortInstalled(string port, bool expectToExist)
    {
        var installed = false;
        for (var i = 0; i< 3; i++)
        {
            installed = GetInstalledPorts().Any(x => x == port);
            if (installed && expectToExist)
            {
                return true;
            }
            await Task.Delay(500);
        }
        return false;
    }

    public async Task InstallComPort()
    {
        if (await CheckIfPortInstalled("COM3", false))
        {
            MessageBox.Show("Port COM3 already registered. Either remove it via Device Manager or uninstall the virutal port.");
            return;
        }
        try
        {
            await ExecuteCommandAsync("setupc.exe", $"install PortName=COM3 PortName=COM23");
            if (await CheckIfPortInstalled("COM3", true))
            {
                MessageBox.Show("Port COM3 successfully installed.");
            }
            else
            {
                MessageBox.Show($"Port COM3 failed to install", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Port COM3 failed to install. {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task UninstallVirtualPorts()
    {

        if (!await CheckIfPortInstalled("COM3", true))
        {
            MessageBox.Show("Port COM3 not found. No need to uninstall.");
            return;
        }
        try
        {
            await ExecuteCommandAsync("setupc.exe", $"uninstall");
            if (!await CheckIfPortInstalled("COM3", false))
            {
                MessageBox.Show("Port COM3 successfully uninstalled.");
            }
            else
            {
                MessageBox.Show($"Port COM3 failed to uninstall. It may be a real device, uninstall it from Device Manager", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Port COM3 failed to uninstall. {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExecuteCommandAsync(string command, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = true, // Necessary for 'runas'
            Verb = "runas", // Request elevation
            WorkingDirectory = @"thirdparty programs\com0com"
        };
        using var process = new Process { StartInfo = processStartInfo };

        process.Start();

        await process.WaitForExitAsync();
    }
}

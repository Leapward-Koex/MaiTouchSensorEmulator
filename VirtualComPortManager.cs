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
        Logger.Info("Trying to install virtual COM port.");
        if (await CheckIfPortInstalled("COM3", false))
        {
            Logger.Warn("Port COM3 already registered.");
            MessageBox.Show("Port COM3 already registered. Either remove it via Device Manager or uninstall the virutal port.");
            return;
        }
        try
        {
            Logger.Info("Calling com0com to install virtual COM ports");
            await ExecuteCommandAsync("setupc.exe", $"install PortName=COM3 PortName=COM23");
            if (await CheckIfPortInstalled("COM3", true))
            {
                Logger.Info("Port COM3 successfully installed.");
                MessageBox.Show("Port COM3 successfully installed.");
            }
            else
            {
                Logger.Error("Port COM3 failed to install");
                MessageBox.Show($"Port COM3 failed to install", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Port COM3 failed to install", ex);
            MessageBox.Show($"Port COM3 failed to install. {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task UninstallVirtualPorts()
    {
        Logger.Info("Trying to uninstall virtual COM port.");
        if (!await CheckIfPortInstalled("COM3", true))
        {
            Logger.Warn("Port COM3 not found. No need to uninstall.");
            MessageBox.Show("Port COM3 not found. No need to uninstall.");
            return;
        }
        try
        {
            Logger.Info("Calling com0com to uninstall virtual COM ports");
            await ExecuteCommandAsync("setupc.exe", $"uninstall");
            if (!await CheckIfPortInstalled("COM3", false))
            {
                Logger.Info("Port COM3 successfully uninstalled.");
                MessageBox.Show("Port COM3 successfully uninstalled.");
            }
            else
            {
                Logger.Error("Port COM3 failed to uninstall");
                MessageBox.Show($"Port COM3 failed to uninstall. It may be a real device, uninstall it from Device Manager", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Port COM3 failed to uninstall", ex);
            MessageBox.Show($"Port COM3 failed to uninstall. {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExecuteCommandAsync(string command, string arguments)
    {
        Logger.Info($"Executing command {command} with arguments {arguments}");
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
        Logger.Info($"Command {command} completed");
    }
}

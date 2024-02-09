﻿using System.Diagnostics;
using System.Text;

namespace WpfMaiTouchEmulator;

internal class VirtualComPortManager
{
    public Task<string> CheckInstalledPortsAsync()
    {
        return ExecuteCommandAsync("thirdparty programs\\com0com\\setupc.exe", $"list");
    }

    public Task<string> InstallComPort()
    {
        return ExecuteCommandAsync("thirdparty programs\\com0com\\setupc.exe", $"install PortName=COM3 PortName=COM23");
    }

    public Task<string> UninstallVirtualPorts()
    {
        return ExecuteCommandAsync("thirdparty programs\\com0com\\setupc.exe", $"uninstall");
    }

    private async Task<string> ExecuteCommandAsync(string command, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = "thirdparty programs\\com0com"
        };

        var outputBuilder = new StringBuilder();
        using var process = new Process { StartInfo = processStartInfo };
        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                outputBuilder.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                outputBuilder.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return outputBuilder.ToString();
    }
}
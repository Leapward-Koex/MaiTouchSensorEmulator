using System.Diagnostics;
using System.IO;
using System.Windows;

namespace WpfMaiTouchEmulator;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        Logger.CleanupOldLogFiles();
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        CreateDump(e.Exception);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        CreateDump(e.ExceptionObject as Exception);
    }

    private void CreateDump(Exception exception)
    {
        var dumpFilePath = Path.Combine(Logger.GetLogPath(), "CrashDump.dmp");

        using (var fs = new FileStream(dumpFilePath, FileMode.Create))
        {
            var process = Process.GetCurrentProcess();
            DumpCreator.MiniDumpWriteDump(process.Handle, (uint)process.Id, fs.SafeFileHandle, DumpCreator.Typ.MiniDumpNormal, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        Logger.Fatal("App encountered a fatal exception", exception);
        MessageBox.Show($"A uncaught exception was thrown: {exception.Message}. \n\n {exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Current.Shutdown(1);
    }

}


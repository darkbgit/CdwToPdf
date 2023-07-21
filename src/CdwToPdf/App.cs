using System.Windows;
using CdwHelper.WPF.Windows;
using MessageBox = System.Windows.MessageBox;

namespace CdwHelper.WPF;

public class App : System.Windows.Application
{
    private readonly MainWindow _mainWindow;

    public App(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        SetupExceptionHandling();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        _mainWindow.Show();
        base.OnStartup(e);
    }

    private void SetupExceptionHandling()
    {
        Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show((e.ExceptionObject as Exception)?.Message);
    }

    private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message);
    }

}
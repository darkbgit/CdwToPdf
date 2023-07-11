using System.Text;
using System.Windows;
using CdwHelper.Core.DI;
using Microsoft.Extensions.DependencyInjection;

namespace CdwHelper.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient(typeof(MainWindow));

        IDependencies dependenciesForCore = new DependenciesForCore();
        dependenciesForCore.RegisterDependencies(serviceCollection);
    }
}
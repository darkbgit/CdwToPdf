using System.IO;
using CdwHelper.Core.DI;
using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;
using CdwHelper.WPF.Helpers;
using CdwHelper.WPF.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CdwHelper.WPF;

public class Program
{
    [STAThread]
    public static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("options.json", false, true)
            .Build();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<App>();
                services.AddSingleton<MainWindow>();
                services.AddScoped<SettingsWindow>();
                services.Configure<PdfOptions>(configuration.GetSection("PdfOptions"));
                ConfigureServices(services);
            })
            .Build();

        var app = host.Services.GetService<App>();
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IWindowFactory, WindowFactory>();

        IDependencies dependenciesForCore = new DependenciesForCore();
        dependenciesForCore.RegisterDependencies(serviceCollection);
    }
}
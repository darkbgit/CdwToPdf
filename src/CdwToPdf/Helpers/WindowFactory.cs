using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace CdwHelper.WPF.Helpers;

public class WindowFactory : IWindowFactory
{
    private readonly IServiceScope _scope;

    public WindowFactory(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();
    }

    public T? Create<T>() where T : Window
    {
        return _scope.ServiceProvider.GetService<T>();
    }
}
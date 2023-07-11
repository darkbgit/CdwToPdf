using System.Windows;

namespace CdwHelper.WPF.Helpers;

public interface IWindowFactory
{
    T? Create<T>()
        where T : Window;
}
using System.Collections.ObjectModel;
using CdwHelper.Core.Models;

namespace CdwHelper.WPF.ViewModels;

public class DrawingsViewModel
{
    public ObservableCollection<KompasDocument> Drawings { get; }

    public DrawingsViewModel()
    {
        Drawings = new ObservableCollection<KompasDocument>();
    }
}
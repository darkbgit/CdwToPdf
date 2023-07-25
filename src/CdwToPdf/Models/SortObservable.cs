using System.Collections.ObjectModel;
using CdwHelper.Core.Models;

namespace CdwHelper.WPF.Models;

public static class SortObservable
{
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var sortableList = new List<T>(collection);
        sortableList.Sort(comparison);

        for (int i = 0; i < sortableList.Count; i++)
        {
            collection.Move(collection.IndexOf(sortableList[i]), i);
        }
    }

    public static void SortByMarking(this ObservableCollection<KompasDocument> collection)
    {
        var sortableList = new List<KompasDocument>(collection);
        var result = sortableList.OrderBy(d => d.Marking)
            .ThenBy(d => d.IsAssemblyDrawing)
            .ToList();

        for (int i = 0; i < result.Count; i++)
        {
            collection.Move(collection.IndexOf(result[i]), i);
        }
    }
}

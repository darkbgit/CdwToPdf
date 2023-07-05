﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CdwToPdf.Models;

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
}

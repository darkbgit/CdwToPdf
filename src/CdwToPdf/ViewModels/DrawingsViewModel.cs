using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Models;

namespace CdwHelper.WPF.ViewModels;

public partial class DrawingsViewModel
{
    [GeneratedRegex("(?:[.])(\\d{2})")]
    private static partial Regex AssemblyAndDetailNumbersRegex();

    public ObservableCollection<KompasDocument> Drawings { get; }

    public DrawingsViewModel()
    {
        Drawings = new ObservableCollection<KompasDocument>();
    }

    public void CheckMarkings()
    {
        if (!Drawings.Any()) return;

        Drawings.First().IsGoodMarking = true;

        var previousMarking = Drawings.First().Marking;

        for (var i = 1; i < Drawings.Count; i++)
        {
            var currentMarking = Drawings[i].Marking;

            var (previousAssemblyNumber, previousDetailNumber) = GetAssemblyAndDetailNumbers(previousMarking);
            var (currentAssemblyNumber, currentDetailNumber) = GetAssemblyAndDetailNumbers(currentMarking);

            var isLast = i == Drawings.Count - 1;

            if (isLast)
            {
                Drawings[i].IsGoodMarking = currentAssemblyNumber == previousAssemblyNumber && currentDetailNumber == previousDetailNumber + 1 ||
                                            currentAssemblyNumber == previousAssemblyNumber + 1 && (Drawings[i].IsAssemblyDrawing || Drawings[i].DrawingType == DocType.Specification) ||
                                            currentAssemblyNumber == previousAssemblyNumber && (Drawings[i].IsAssemblyDrawing || Drawings[i].DrawingType == DocType.Specification);
                continue;
            }

            var nextMarking = Drawings[i + 1].Marking;
            var (nextAssemblyNumber, nextDetailNumber) = GetAssemblyAndDetailNumbers(nextMarking);

            Drawings[i].IsGoodMarking =
                currentAssemblyNumber == previousAssemblyNumber && currentDetailNumber == previousDetailNumber + 1 ||
                currentAssemblyNumber == previousAssemblyNumber && currentDetailNumber == previousDetailNumber
                                                                && (Drawings[i].IsAssemblyDrawing ||
                                                                    Drawings[i].DrawingType == DocType.Specification) ||
                currentAssemblyNumber == previousAssemblyNumber + 1 && (Drawings[i].IsAssemblyDrawing ||
                                                                        Drawings[i].DrawingType ==
                                                                        DocType.Specification);

            if (!Drawings[i].IsGoodMarking)
            {
                if (nextAssemblyNumber == currentAssemblyNumber && nextDetailNumber == currentDetailNumber + 1 ||
                    nextAssemblyNumber == currentAssemblyNumber && nextDetailNumber == currentDetailNumber
                                                                && (Drawings[i + 1].IsAssemblyDrawing || Drawings[i + 1].DrawingType == DocType.Specification) ||
                    nextAssemblyNumber == currentAssemblyNumber + 1 && (Drawings[i + 1].IsAssemblyDrawing ||
                                                                        Drawings[i + 1].DrawingType ==
                                                                        DocType.Specification))
                {
                    Drawings[i - 1].IsGoodMarking = false;
                }
            }

            previousMarking = currentMarking;
        }
    }

    private static (int assembly, int detail) GetAssemblyAndDetailNumbers(string marking)
    {
        var regex = AssemblyAndDetailNumbersRegex();
        var matches = regex.Matches(marking);
        var assembly = Convert.ToInt32(matches[0].Groups[1].Value);
        var detail = Convert.ToInt32(matches[1].Groups[1].Value);

        return (assembly, detail);
    }
}
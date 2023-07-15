using CdwHelper.Core.Models;

namespace CdwHelper.Core.Helpers;

public static class DrawingsHelpers
{
    public static string CalculateFormats(IEnumerable<KompasDocument> documents)
    {
        var kompasDocuments = documents.ToList();

        var result = kompasDocuments
            .SelectMany(d => d.Formats)
            .GroupBy(f => f.DrawingFormat)
            .OrderByDescending(f => f.Key)
            .Select(g => new { Format = g.Key, Count = g.Sum(f => f.Count * f.SheetsCount) })
            .ToList();

        return string.Join(Environment.NewLine, result.Select(i => $"{i.Format} - {i.Count}"));
    }
}
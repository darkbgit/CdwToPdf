using CdwHelper.Core.Enums;

namespace CdwHelper.Core.Models;

public class KompasDocument
{
    public bool IsAssemblyDrawing { get; set; }
    public DocType DrawingType { get; set; }
    public int SheetsNumber { get; set; }
    public KompasVersion AppVersion { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string CheckedBy { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FullFileName { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Marking { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RateOfInspection { get; set; } = string.Empty;
    public string StampAuthor { get; set; } = string.Empty;
    public IEnumerable<Format> Formats { get; set; } = Enumerable.Empty<Format>();

    public bool IsGoodFullFileName { get; set; }

    public string FormatsName => string.Join(", ", Formats);
    //public bool IsGoodName =>
    //    Path.Split('\\').Last()[..^4] == Designation + " - " + Name;

    //public override string ToString()
    //{
    //    return Designation + (IsAssemblyDrawing && DrawingType == DocType.Drawing ? " СБ" : "") + " - " + Name;
    //}
}

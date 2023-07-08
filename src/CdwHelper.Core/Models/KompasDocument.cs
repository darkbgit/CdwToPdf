using System.ComponentModel;
using System.Runtime.CompilerServices;
using CdwHelper.Core.Enums;

namespace CdwHelper.Core.Models;

public class KompasDocument : INotifyPropertyChanged
{
    private bool _isGoodMarking = true;

    public bool IsAssemblyDrawing { get; set; }
    public DocType DrawingType { get; set; }
    public int SheetsNumber { get; set; }
    public KompasVersion AppVersion { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string CheckedBy { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FullFileName { get; set; } = string.Empty;
    //public string Format { get; set; } = string.Empty;
    public string Marking { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RateOfInspection { get; set; } = string.Empty;
    public string StampAuthor { get; set; } = string.Empty;
    public IEnumerable<Format> Formats { get; set; } = Enumerable.Empty<Format>();
    public string FormatsName => string.Join(", ", Formats);

    public bool IsGoodMarking
    {
        get => _isGoodMarking;
        set
        {
            _isGoodMarking = value;
            OnPropertyChanged();
        }
    }

    public bool IsGoodFullFileName { get; set; }

    public bool IsGoodFormats =>
        DrawingType switch
        {
            DocType.Drawing => Formats.Sum(f => f.Count) == SheetsNumber,
            DocType.Specification => Formats.Count() == 1 && Formats.First().SheetsCount == SheetsNumber,
            DocType.DrawingUser or DocType.Fragment or DocType.Assembly3D or DocType.Part3D or DocType.Txt or DocType.TxtUser or DocType.TechnologyAssemble3D => true,
            _ => false,
        };


    //public bool IsGoodName =>
    //    Path.Split('\\').Last()[..^4] == Designation + " - " + Name;

    //public override string ToString()
    //{
    //    return Designation + (IsAssemblyDrawing && DrawingType == DocType.Drawing ? " СБ" : "") + " - " + Name;
    //}
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

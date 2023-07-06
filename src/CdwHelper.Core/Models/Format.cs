using System.Text;
using CdwHelper.Core.Enums;

namespace CdwHelper.Core.Models;

public class Format
{
    public DrawingFormat DrawingFormat { get; set; }

    public int Count { get; set; } = 1;

    public int AdditionalNumber { get; set; } = 1;

    public int SheetsCount { get; set; } = 1;

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Count > 1)
        {
            sb.Append($"{Count}x");
        }

        sb.Append(DrawingFormat);

        if (AdditionalNumber > 1)
        {
            sb.Append($"x{AdditionalNumber}");
        }

        return sb.ToString();
    }
}
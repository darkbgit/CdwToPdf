using System.ComponentModel;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Interfaces;

public interface IPdfConverter
{
    IEnumerable<string> ConvertFiles(IEnumerable<KompasDocument> documents, BackgroundWorker worker,
        DrawingFormat format = DrawingFormat.All);
}
using System.ComponentModel;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Interfaces;

public interface IPdfConverter
{
    /// <summary>
    /// Convert all KompasDocuments to one .pdf file.
    /// </summary>
    /// <param name="documents"></param>
    /// <param name="format"></param>
    /// <returns>Converting errors.</returns>
    IEnumerable<string> ConvertFiles(IEnumerable<KompasDocument> documents, DrawingFormat format = DrawingFormat.All);

    /// <summary>
    /// Convert all KompasDocuments to one .pdf file.
    /// </summary>
    /// <param name="documents"></param>
    /// <param name="worker"></param>
    /// <param name="format"></param>
    /// <returns>Converting errors.</returns>
    IEnumerable<string> ConvertFiles(IEnumerable<KompasDocument> documents, BackgroundWorker? worker,
        DrawingFormat format = DrawingFormat.All);
}
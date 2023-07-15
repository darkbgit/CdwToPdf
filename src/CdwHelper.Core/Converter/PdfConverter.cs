using System.ComponentModel;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;
using Microsoft.Extensions.Options;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CdwHelper.Core.Converter;

internal class PdfConverter : IPdfConverter
{
    private readonly IOptionsMonitor<PdfOptions> _pdfOptionsMonitor;
    private const string PdfExtension = ".pdf";

    public PdfConverter(IOptionsMonitor<PdfOptions> pdfOptionsMonitor)
    {
        _pdfOptionsMonitor = pdfOptionsMonitor;
    }

    public IEnumerable<string> ConvertFiles(IEnumerable<KompasDocument> documents,
        DrawingFormat format = DrawingFormat.All)
    {
        return ConvertFiles(documents, null, format);
    }

    public IEnumerable<string> ConvertFiles(IEnumerable<KompasDocument> documents, BackgroundWorker? worker,
        DrawingFormat formatsToConvert = DrawingFormat.All)
    {
        var errors = new List<string>();
        int progressValue = 0;

        var kompasDocuments = documents.ToList();

        //if (!kompasDocuments.Any())
        //    return;

        var pathToKompasPdfConverter = _pdfOptionsMonitor.CurrentValue.PathToPdf2d;
        var kompasApi = _pdfOptionsMonitor.CurrentValue.KompasAPI;

        var converter = DrawingConverterFactory.GetConverter(pathToKompasPdfConverter, kompasApi)
            ?? throw new Exception("Couldn't create Kompas converter.");

        worker?.ReportProgress(++progressValue);

        using var targetDoc = new PdfDocument();

        foreach (var file in kompasDocuments)
        {
            if (!file.Formats.Any(f => f.DrawingFormat != DrawingFormat.Undefined && formatsToConvert.HasFlag(f.DrawingFormat)))
            {
                worker?.ReportProgress(++progressValue);
                continue;
            }

            var lastIndex = file.FullFileName.LastIndexOf('\\');

            if (lastIndex == -1)
            {
                errors.Add($"Wrong path for file {file.FullFileName}");
                worker?.ReportProgress(++progressValue);
                continue;
            }

            var pdfFilePath = file.FullFileName[..(lastIndex + 1)] + file.Name + PdfExtension;

            if (converter.Convert(file.FullFileName, pdfFilePath, 0, false) == 0)
            {
                errors.Add($"Couldn't convert to pdf file {file.FullFileName}");
                worker?.ReportProgress(++progressValue);
                continue;
            }

            using var pdfDoc = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
            try
            {
                for (var i = 0; i < pdfDoc.PageCount; i++)
                {
                    var pageFormat = GetPageFormat(pdfDoc.Pages[i].Width.Millimeter, pdfDoc.Pages[i].Height.Millimeter);

                    if (pageFormat == DrawingFormat.Undefined || !formatsToConvert.HasFlag(pageFormat))
                    {
                        continue;
                    }

                    targetDoc.AddPage(pdfDoc.Pages[i]);
                }
            }
            catch (Exception)
            {
                errors.Add($"Couldn't add to pdf file {pdfFilePath}");
            }
            finally
            {
                pdfDoc.Close();

                File.Delete(pdfFilePath);

                worker?.ReportProgress(++progressValue);
            }
        }

        if (targetDoc.PageCount == 0)
        {
            worker?.ReportProgress(++progressValue);
            throw new Exception("Pdf document have not any page. File don't saved.");
        }

        var first = kompasDocuments.First();

        var filePrefix = formatsToConvert switch
        {
            DrawingFormat.Undefined => string.Empty,
            DrawingFormat.A0 => "_A0",
            DrawingFormat.A1 => "_A1",
            DrawingFormat.A2 => "_A2",
            DrawingFormat.A3 => "_A3",
            DrawingFormat.A4 => "_A4",
            DrawingFormat.A5 => "_A5",
            DrawingFormat.All => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(formatsToConvert), formatsToConvert, null)
        };

        var newFilepath = first.FullFileName
            [..(kompasDocuments.First().FullFileName.LastIndexOf('\\') + 1)]
                          + first.Marking
                          + " - "
                          + first.Name
                          + filePrefix
                          + PdfExtension;


        targetDoc.Save(newFilepath);

        return errors;
    }

    private static DrawingFormat GetPageFormat(double width, double height)
    {
        const int accuracy = 4;

        (int height, int width) a5PortraitSize = (210, 148);
        (int height, int width) a4PortraitSize = (297, 210);
        (int height, int width) a3PortraitSize = (420, 297);
        (int height, int width) a2PortraitSize = (594, 420);
        (int height, int width) a1PortraitSize = (841, 594);
        (int height, int width) a0PortraitSize = (1189, 841);


        switch (true)
        {
            case true when CheckSizeWithAccuracy(width, a5PortraitSize.width, accuracy) && CheckSizeWithAccuracy(height, a5PortraitSize.height, accuracy):
            case true when CheckSizeWithAccuracy(width, a5PortraitSize.height, accuracy) && CheckSizeWithAccuracy(height, a5PortraitSize.width, accuracy):
                return DrawingFormat.A5;
            case true when CheckSizeWithAccuracy(width, a4PortraitSize.width, accuracy) && CheckSizeWithAccuracy(height, a4PortraitSize.height, accuracy):
            case true when CheckSizeWithAccuracy(width, a4PortraitSize.height, accuracy) && CheckSizeWithAccuracy(height, a4PortraitSize.width, accuracy):
                return DrawingFormat.A4;
            case true when CheckSizeWithAccuracy(width, a3PortraitSize.width, accuracy) && CheckSizeWithAccuracy(height, a3PortraitSize.height, accuracy):
            case true when CheckSizeWithAccuracy(width, a3PortraitSize.height, accuracy) && CheckSizeWithAccuracy(height, a3PortraitSize.width, accuracy):
                return DrawingFormat.A3;
            case true when CheckSizeWithAccuracy(width, a2PortraitSize.width, accuracy) && CheckSizeWithAccuracy(height, a2PortraitSize.height, accuracy):
            case true when CheckSizeWithAccuracy(width, a2PortraitSize.height, accuracy) && CheckSizeWithAccuracy(height, a2PortraitSize.width, accuracy):
                return DrawingFormat.A2;
            case true when CheckSizeWithAccuracy(width, a1PortraitSize.width, accuracy) && CheckSizeWithAccuracy(height, a1PortraitSize.height, accuracy):
            case true when CheckSizeWithAccuracy(width, a1PortraitSize.height, accuracy) && CheckSizeWithAccuracy(height, a1PortraitSize.width, accuracy):
                return DrawingFormat.A1;
            case true when CheckSizeWithAccuracy(width, a0PortraitSize.width, accuracy) && CheckSizeWithAccuracy(height, a0PortraitSize.height, accuracy):
            case true when CheckSizeWithAccuracy(width, a0PortraitSize.height, accuracy) && CheckSizeWithAccuracy(height, a0PortraitSize.width, accuracy):
                return DrawingFormat.A0;
            default:
                return DrawingFormat.Undefined;
        }
    }

    private static bool CheckSizeWithAccuracy(double value, int defaultValue, int accuracy)
    {
        return value > (defaultValue - accuracy) && value < (defaultValue + accuracy);
    }
}

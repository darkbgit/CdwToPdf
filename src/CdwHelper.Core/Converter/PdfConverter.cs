using System.ComponentModel;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CdwHelper.Core.Converter;

public class PdfConverter : IPdfConverter
{
    private const string KompasApi = "KOMPAS.Application.7";
    private const string KompasPathPdfConverter = @"C:\Program Files\ASCON\KOMPAS-3D v21\Bin\Pdf2d.dll";
    private const string PdfExtension = ".pdf";

    public IEnumerable<string> ConvertFiles(IEnumerable<KompasDocument> documents, BackgroundWorker worker,
        DrawingFormat format = DrawingFormat.All)
    {
        var errors = new List<string>();
        int value = 0;

        var kompasDocuments = documents.ToList();

        //if (!kompasDocuments.Any())
        //    return;

        var converter = DrawingConverterFactory.GetConverter(KompasPathPdfConverter, KompasApi)
            ?? throw new Exception("Couldn't create Kompas converter.");

        worker.ReportProgress(++value);

        using var targetDoc = new PdfDocument();

        foreach (var file in kompasDocuments)
        {
            if (!file.Formats.Any(f => format.HasFlag(f.DrawingFormat))) continue;

            var lastIndex = file.FullFileName.LastIndexOf('\\');

            if (lastIndex == -1)
            {
                errors.Add($"Wrong path for file {file.FullFileName}");
                continue;
            }

            var pdfFilePath = file.FullFileName[..(lastIndex + 1)] + file.Name + PdfExtension;

            if (converter.Convert(file.FullFileName, pdfFilePath, 0, false) == 0)
            {
                errors.Add($"Couldn't convert to pdf file {file.FullFileName}");
                continue;
            }

            using var pdfDoc = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
            try
            {
                for (var i = 0; i < pdfDoc.PageCount; i++)
                {
                    var format = GetPageFormat(pdfDoc.Pages[i].Width.Millimeter, pdfDoc.Pages[i].Height.Millimeter, pdfDoc.Pages[i].Orientation);
                    targetDoc.AddPage(pdfDoc.Pages[i]);
                }
            }
            catch (Exception)
            {
                errors.Add($"Couldn't add to pdf file {pdfFilePath}");
            }

            pdfDoc.Close();

            File.Delete(pdfFilePath);

            worker.ReportProgress(++value);
        }

        if (targetDoc.PageCount == 0)
        {
            worker.ReportProgress(++value);
            throw new Exception("Pdf document have not any page. File don't saved.");
        }

        var first = kompasDocuments.First();

        var newFilepath = first.FullFileName
            [..(kompasDocuments.First().FullFileName.LastIndexOf('\\') + 1)] + first.Marking + " - " + first.Name;
        targetDoc.Save(newFilepath + PdfExtension);

        return errors;
    }

    private static DrawingFormat GetPageFormat(double width, double height, PageOrientation orientation)
    {
        (int height, int width) a4PortraitSize = (297, 214);

        if (orientation == PageOrientation.Portrait)
        {
            if (width is > (a4PortraitSize.width - 2) and < a4PortraitSize.width + 2)
            {
                return DrawingFormat.A4;
            }
        }
        return DrawingFormat.Undefined;
    }
}

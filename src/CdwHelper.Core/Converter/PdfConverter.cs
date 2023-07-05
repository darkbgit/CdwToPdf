using CdwHelper.Core.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CdwHelper.Core.Converter;

internal class PdfConverter
{
    private const string KOMPAS_API = "KOMPAS.Application.7";
    //private const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v20\Bin\Pdf2d.dll";
    private const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v21\Bin\Pdf2d.dll";
    private const string PDF_EXTENSION = ".pdf";

    private void ConvertFiles(IEnumerable<KompasDocument> documents)
    {
        if (!documents.Any())
            return;

        var converter = DrawingConverterFactory.GetConverter(KOMPAS_PATH_PDF_CONVERTER, KOMPAS_API)
            ?? throw new Exception("Couldn't create Kompas converter.");

        using var targetDoc = new PdfDocument();

        foreach (var file in documents)
        {
            var lastIndex = file.FullFileName.LastIndexOf('\\');

            if (lastIndex == -1)
            {
                //MessageBox.Show($"Wrong path for file {file.Path}");
                continue;
            }

            var pdfFilePath = file.FullFileName[..(lastIndex + 1)] + file.FileName + ".pdf";

            if (converter.Convert(file.FullFileName, pdfFilePath, 0, false) == 0)
            {
                //MessageBox.Show($"Couldn't convert to pdf file {file.Path}");
            }

            using var pdfDoc = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
            try
            {
                for (var i = 0; i < pdfDoc.PageCount; i++)
                {
                    targetDoc.AddPage(pdfDoc.Pages[i]);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show($"Couldn't add to pdf file {pdfFilePath}");
            }


            pdfDoc.Close();

            File.Delete(pdfFilePath);
        }

        var first = documents.First();

        var newFilepath = first.FullFileName
            [..(documents.First().FullFileName.LastIndexOf('\\') + 1)] + first.Marking + " - " + first.Name;
        targetDoc.Save(newFilepath + ".pdf");

        //MessageBox.Show("Completed");
    }
}

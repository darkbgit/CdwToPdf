using System.IO.Compression;
using CdwHelper.Core.Interfaces;

namespace CdwHelper.Core.Analyzers;

internal class ZipAnalyzer : IZipAnalyzer
{
    private readonly string _filePath;

    public ZipAnalyzer(string filePath)
    {
        _filePath = filePath;
    }

    public Stream GetRootPart()
    {
        using var packageStream = new MemoryStream();

        using (FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fs.CopyTo(packageStream);
        }

        packageStream.Position = 0;

        using var z = new ZipArchive(packageStream, ZipArchiveMode.Read);

        var infoEntry = z.GetEntry("MetaProductInfo")
            ?? throw new Exception("ZipFile entry not found.");

        var result = CopyToStreamAndSetToBegin(infoEntry.Open());

        return result;
    }

    public Stream GetVersionPart()
    {
        using var packageStream = new MemoryStream();

        using (FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fs.CopyTo(packageStream);
        }

        packageStream.Position = 0;

        using var zipArchive = new ZipArchive(packageStream, ZipArchiveMode.Read);
        var fileInfo = zipArchive.GetEntry("FileInfo")
                       ?? throw new Exception("AppVersion couldn't find");

        var result = CopyToStreamAndSetToBegin(fileInfo.Open());

        return result;
    }

    private static Stream CopyToStreamAndSetToBegin(Stream stream)
    {
        Stream result = new MemoryStream();
        stream.CopyTo(result);
        result.Position = 0;

        return result;
    }
}

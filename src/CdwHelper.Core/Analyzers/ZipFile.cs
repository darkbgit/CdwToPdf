using System.IO.Compression;
using CdwHelper.Core.Exceptions;

namespace CdwHelper.Core.Analyzers;

internal class ZipFile
{
    private readonly string _filePath;

    public ZipFile(string filePath)
    {
        _filePath = filePath;
    }

    public Stream GetRootPartStream()
    {
        const string rootEntryName = "MetaProductInfo";

        using var packageStream = new MemoryStream();

        try
        {
            using FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.CopyTo(packageStream);
        }
        catch (IOException e)
        {
            throw new AnalyzeException(e.Message);
        }


        packageStream.Position = 0;

        using var z = new ZipArchive(packageStream, ZipArchiveMode.Read);

        var infoEntry = z.GetEntry(rootEntryName)
            ?? throw new AnalyzeException("ZipFile entry not found.");

        var result = CopyToStreamAndSetToBegin(infoEntry.Open());

        return result;
    }

    public Stream GetVersionPartStream()
    {
        const string fileInfoEntryName = "FileInfo";

        using var packageStream = new MemoryStream();

        try
        {
            using FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.CopyTo(packageStream);
        }
        catch (IOException e)
        {
            throw new AnalyzeException(e.Message);
        }


        packageStream.Position = 0;

        using var zipArchive = new ZipArchive(packageStream, ZipArchiveMode.Read);
        var fileInfo = zipArchive.GetEntry(fileInfoEntryName)
                       ?? throw new AnalyzeException("AppVersion couldn't find");

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

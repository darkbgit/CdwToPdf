using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Analyzers;

public class FileAnalyzer : IFileAnalyzer
{
    public KompasDocument Analyze(string filePath)
    {
        IZipAnalyzer zip = new ZipAnalyzer(filePath);

        using var versionPart = zip.GetVersionPart();

        IVersionPartAnalyzer versionPartAnalyzer = new VersionPartAnalyzer(versionPart);

        IRootPartAnalyzer rootPartAnalyzer = RootPartAnalyzerFactory.GetRootAnalyzer(versionPartAnalyzer.AppVersion);

        using var rootPart = zip.GetRootPart();

        var document = rootPartAnalyzer.AnalyzeXml(rootPart, versionPartAnalyzer.DocType);

        document.DrawingType = versionPartAnalyzer.DocType;
        document.AppVersion = versionPartAnalyzer.AppVersion;

        document.IsGoodFullFileName = document.FullFileName == filePath;

        if (!document.IsGoodFullFileName)
        {
            document.FullFileName = filePath;
        }

        return document;
    }
}

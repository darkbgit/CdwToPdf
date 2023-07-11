using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Analyzers;

internal class FileAnalyzer : IFileAnalyzer
{
    public KompasDocument Analyze(string filePath)
    {
        var zipFile = new ZipFile(filePath);

        using var versionPartStream = zipFile.GetVersionPartStream();

        var versionPart = new VersionPart(versionPartStream);

        IRootPartAnalyzer rootPartAnalyzer = RootPartAnalyzerFactory.GetRootAnalyzer(versionPart.AppVersion);

        using var rootPart = zipFile.GetRootPartStream();

        var document = rootPartAnalyzer.AnalyzeXml(rootPart, versionPart.DocType);

        document.AppVersion = versionPart.AppVersion;

        document.IsGoodFullFileName = document.FullFileName == filePath;

        if (!document.IsGoodFullFileName)
        {
            document.FullFileName = filePath;
        }

        return document;
    }
}

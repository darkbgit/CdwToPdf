using CdwHelper.Core.Enums;
using CdwHelper.Core.Interfaces;

namespace CdwHelper.Core.Analyzers;

internal class VersionPartAnalizer : IVersionPartAnalyzer
{
    private const string fileType = "FileType=";
    private const string appVersion = "AppVersion=KOMPAS_";
    private const char newline = '\n';
    private readonly Stream _xmlStream;

    public VersionPartAnalizer(Stream xmlStream)
    {
        _xmlStream = xmlStream;
        AppVersion = GetAppVersion();
        DocType = GetDocType();
    }

    public KompasVersion AppVersion { get; init; }

    public DocType DocType { get; init; }

    private KompasVersion GetAppVersion()
    {
        var stream = CopyToStreamAndSetToBegin(_xmlStream);

        using var reader = new StreamReader(stream);
        var info = reader.ReadToEnd().Split(newline);

        var version = info
            .FirstOrDefault(s => s.Contains(appVersion))
            ?[appVersion.Length..];

        if (string.IsNullOrEmpty(version))
            throw new Exception("Couldn't get Kompas version in which file was created.");

        var komasVersion = version switch
        {
            "19.0" => KompasVersion.V19,
            "20.0" => KompasVersion.V20,
            "21.0" => KompasVersion.V21,
            _ => throw new Exception($"Wrong version {version}.")
        };

        return komasVersion;
    }

    private DocType GetDocType()
    {
        var stream = CopyToStreamAndSetToBegin(_xmlStream);

        using (var reader = new StreamReader(stream))
        {
            var info = reader.ReadToEnd().Split(newline);

            if (int.TryParse(info.FirstOrDefault(s => s.Contains(fileType))?[^1].ToString(), out int type))
            {
                return (DocType)type;
            }
        }

        throw new Exception("Gouldn't get Kompas file type.");
    }

    private static Stream CopyToStreamAndSetToBegin(Stream stream)
    {
        Stream result = new MemoryStream();
        stream.CopyTo(result);
        result.Position = 0;
        stream.Position = 0;

        return result;
    }
}
using CdwHelper.Core.Enums;

namespace CdwHelper.Core.Analyzers;

internal class VersionPart
{
    private const string FileTypeName = "FileType=";
    private const string AppVersionName = "AppVersion=KOMPAS_";
    private const char Newline = '\n';
    private readonly Stream _xmlStream;

    public VersionPart(Stream xmlStream)
    {
        _xmlStream = xmlStream;
        AppVersion = GetAppVersion();
        DocType = GetDocType();
    }

    public KompasVersion AppVersion { get; }

    public DocType DocType { get; }

    private KompasVersion GetAppVersion()
    {
        var stream = CopyToStreamAndSetToBegin(_xmlStream);

        using var reader = new StreamReader(stream);
        var info = reader.ReadToEnd().Split(Newline);

        var version = info
            .FirstOrDefault(s => s.Contains(AppVersionName))
            ?[AppVersionName.Length..];

        if (string.IsNullOrEmpty(version))
            throw new Exception("Couldn't get Kompas version in which file was created.");

        var kompasVersion = version switch
        {
            "19.0" => KompasVersion.V19,
            "20.0" => KompasVersion.V20,
            "21.0" => KompasVersion.V21,
            _ => throw new Exception($"Wrong version {version}.")
        };

        return kompasVersion;
    }

    private DocType GetDocType()
    {
        var stream = CopyToStreamAndSetToBegin(_xmlStream);

        using var reader = new StreamReader(stream);

        var info = reader.ReadToEnd()
            .Split(Newline);

        if (Enum.TryParse<DocType>(info.FirstOrDefault(s => s.Contains(FileTypeName))?[^1].ToString(), out var type))
        {
            return type;
        }

        throw new Exception("Couldn't get Kompas file type.");
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
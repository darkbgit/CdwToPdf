namespace CdwHelper.Core.Interfaces;

internal interface IZipAnalyzer
{
    Stream GetVersionPart();
    Stream GetRootPart();
}

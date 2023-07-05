using CdwHelper.Core.Enums;

namespace CdwHelper.Core.Interfaces;

internal interface IVersionPartAnalyzer
{
    KompasVersion AppVersion { get; }

    DocType DocType { get; }
}

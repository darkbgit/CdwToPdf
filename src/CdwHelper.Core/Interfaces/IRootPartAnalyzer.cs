using CdwHelper.Core.Enums;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Interfaces;

internal interface IRootPartAnalyzer
{
    KompasDocument AnalyzeXml(Stream xml, DocType type);
}

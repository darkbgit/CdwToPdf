using CdwHelper.Core.Analyzers.Ver20;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Interfaces;

namespace CdwHelper.Core.Analyzers;

internal class RootPartAnalyzerFactory
{
    public static IRootPartAnalyzer GetRootAnalyzer(KompasVersion version)
    {
        IRootPartAnalyzer rootPartAnalyzer = version switch
        {
            KompasVersion.V19 => throw new NotImplementedException(),
            KompasVersion.V20 => new RootPartAnalyzer20(),
            KompasVersion.V21 => new RootPartAnalyzer20(),
            KompasVersion.V22 => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        return rootPartAnalyzer;
    }
}

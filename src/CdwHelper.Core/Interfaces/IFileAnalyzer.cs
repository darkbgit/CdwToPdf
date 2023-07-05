using CdwHelper.Core.Models;

namespace CdwHelper.Core.Interfaces;

public interface IFileAnalyzer
{
    KompasDocument Analyze(string filePath);
}

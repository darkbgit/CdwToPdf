using CdwHelper.Core.Exceptions;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Interfaces;

public interface IFileAnalyzer
{
    /// <summary>
    /// Analyze .cdw, .spc, m3d, .a3d files.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns><see cref="KompasDocument"/></returns>
    /// <exception cref="AnalyzeException"></exception>
    KompasDocument Analyze(string filePath);
}

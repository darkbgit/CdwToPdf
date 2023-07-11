using CdwHelper.Core.Analyzers;
using CdwHelper.Core.Converter;
using CdwHelper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CdwHelper.Core.DI;

public class DependenciesForCore : IDependencies
{
    public void RegisterDependencies(IServiceCollection services)
    {
        services.AddTransient<IFileAnalyzer, FileAnalyzer>();

        services.AddTransient<IPdfConverter, PdfConverter>();
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace CdwHelper.Core.Interfaces;

public interface IDependencies
{
    void RegisterDependencies(IServiceCollection services);
}
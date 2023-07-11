using Microsoft.Extensions.DependencyInjection;

namespace CdwHelper.Core.DI;

public interface IDependencies
{
    void RegisterDependencies(IServiceCollection services);
}
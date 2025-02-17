using Bounteous.Core.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bounteous.Data;

public class ModuleStartup : IModule
{
    public void RegisterServices(IServiceCollection services)
        => services.TryAddSingleton<IDbContextObserver, DefaultDbContextObserver>();

    public int Priority => 1;
}
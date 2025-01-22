using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xerris.DotNet.Core.DI;

namespace Bounteous.DotNet.Data;

public class ModuleStartup : IModule
{
    public void RegisterServices(IServiceCollection services)
        => services.TryAddSingleton<IDbContextObserver, DefaultDbContextObserver>();

    public int Priority => 1;
}
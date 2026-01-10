using Bounteous.Core;
using Bounteous.Core.DI;
using Bounteous.Data;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Infrastructure;
using Bounteous.Data.Sample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample;

public static class AppStartup
{
    public static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IConnectionStringProvider, SampleConnectionStringProvider>();
        services.AddSingleton<IConnectionBuilder, ConnectionBuilder>();
        
        // Register ModuleStartup first to get default registrations (including IdentityProvider)
        new ModuleStartup().RegisterServices(services);
        
        // The built-in IdentityProvider<Guid> is now registered by ModuleStartup
        // We can resolve it and use it in the factory
        services.AddScoped<IDbContextFactory<SampleDbContext, Guid>>(sp =>
        {
            var connectionBuilder = sp.GetRequiredService<IConnectionBuilder>();
            var observer = sp.GetRequiredService<IDbContextObserver>();
            var identityProvider = sp.GetRequiredService<IIdentityProvider<Guid>>();
            
            return new SampleDbContextFactory(connectionBuilder, observer, identityProvider);
        });

        services.AutoRegister(typeof(Program).Assembly);

        return services;
    }

    public static void InitializeLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/sample-.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 10_485_760,
                retainedFileCountLimit: 14,
                shared: false,
                buffered: true
            )
            .MinimumLevel.Debug()
            .CreateLogger();
    }
}

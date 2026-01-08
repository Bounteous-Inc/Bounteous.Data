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
        services.AddScoped<IDbContextFactory<SampleDbContext>, SampleDbContextFactory>();

        new ModuleStartup().RegisterServices(services);

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

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

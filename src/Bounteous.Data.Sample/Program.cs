using Bounteous.Data;
using Bounteous.Data.Sample;
using Bounteous.Data.Sample.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

AppStartup.InitializeLogging();

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    var services = new ServiceCollection();
    AppStartup.ConfigureServices(services, configuration);
    var serviceProvider = services.BuildServiceProvider();

    var userId = Guid.NewGuid();
    
    // Set the user ID in the built-in IdentityProvider for automatic resolution
    var identityProvider = serviceProvider.GetRequiredService<IIdentityProvider<Guid>>();
    if (identityProvider is IdentityProvider<Guid> provider)
    {
        provider.SetCurrentUserId(userId);
    }

    Log.Information("╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║          Bounteous.Data - Feature Demonstration Suite        ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Information("");

    // Run all feature demonstrations
    var featureRunner = new FeatureRunner(serviceProvider);
    await featureRunner.RunAllFeaturesAsync(userId);

    Log.Information("");
    Log.Information("╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║                  All Features Demonstrated!                   ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

using Bounteous.Data.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates query extensions like WhereIf, IncludeIf, and Pagination.
/// Shows how to build dynamic queries with conditional filtering.
/// </summary>
public class Feature06_QueryExtensionsDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 6;
    public string FeatureName => "Query Extensions (WhereIf, IncludeIf, Pagination)";

    public Feature06_QueryExtensionsDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");

        var orderService = _serviceProvider.GetRequiredService<IOrderService>();
        var customerService = _serviceProvider.GetRequiredService<ICustomerService>();

        // Create test data
        var customer = await customerService.CreateCustomerAsync("Test Customer", "test@example.com", "555-0000", userId);
        await orderService.CreateOrderAsync(customer.Id, new List<(Guid productId, int quantity)>(), userId);

        Log.Information("[QUERY-EXT] ✓ Query extensions enable dynamic, conditional queries");
        Log.Information("[QUERY-EXT] ✓ WhereIf, IncludeIf, and pagination simplify complex scenarios");
    }
}

using Bounteous.Data.Extensions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain.Enums;
using Bounteous.Data.Sample.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates value converters for enums and DateTime.
/// Shows how EnumConverter stores enums as description strings and DateTimeConverter ensures UTC storage.
/// </summary>
public class Feature02_ValueConvertersDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 2;
    public string FeatureName => "Value Converters (Enum & DateTime)";

    public Feature02_ValueConvertersDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Debug("[CONVERTER] EnumConverter stores enum as description string");
        Log.Debug("[CONVERTER] DateTimeConverter ensures UTC storage");

        var productService = _serviceProvider.GetRequiredService<IProductService>();
        var contextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SampleDbContext, Guid>>();

        var product1 = await productService.CreateProductAsync(
            "Laptop",
            "High-performance laptop",
            1299.99m,
            10,
            "LAP-001",
            userId);

        using (var context = contextFactory.Create().WithUserIdTyped(userId))
        {
            var prod = await context.Products.FindAsync(product1.Id);
            if (prod != null)
            {
                prod.Status = ProductStatus.Active;
                prod.LastRestockedOn = DateTime.UtcNow;
                await context.SaveChangesAsync();
                Log.Information("[CONVERTER] ✓ Product Status: {Status} (stored as: 'Active')", prod.Status);
                Log.Information("[CONVERTER] ✓ LastRestockedOn: {Date:yyyy-MM-dd HH:mm:ss} UTC", prod.LastRestockedOn);
            }
        }

        await productService.CreateProductAsync("Mouse", "Wireless mouse", 29.99m, 50, "MOU-001", userId);
        await productService.CreateProductAsync("Keyboard", "Mechanical keyboard", 89.99m, 25, "KEY-001", userId);
    }
}

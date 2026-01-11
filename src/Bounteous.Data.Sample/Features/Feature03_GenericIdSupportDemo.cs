using Bounteous.Data.Extensions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates generic ID support with different types (Guid, long, int).
/// Shows how AuditBase works with different ID types like AuditBase&lt;long&gt;.
/// </summary>
public class Feature03_GenericIdSupportDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 3;
    public string FeatureName => "Generic ID Support (Guid, long, int)";

    public Feature03_GenericIdSupportDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Debug("[GENERIC-ID] Demonstrating AuditBase<long> with Category entity");

        var contextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SampleDbContext, Guid>>();

        using (var context = contextFactory.Create().WithUserIdTyped(userId))
        {
            var category = new Category
            {
                Name = "Electronics",
                Description = "Electronic devices and accessories"
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            Log.Information("[GENERIC-ID] ✓ Category Created with long ID: {Id}", category.Id);
            Log.Information("[GENERIC-ID]   - CreatedBy: {CreatedBy} (Guid type)", category.CreatedBy);
            Log.Information("[GENERIC-ID]   - Demonstrates AuditBase<long, Guid> flexibility");
        }
    }
}

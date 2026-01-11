using Bounteous.Data.Domain.ReadOnly;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates ReadOnlyDbSet for immediate fail-fast validation.
/// Shows how ReadOnlyDbSet prevents modifications at the DbSet level while supporting
/// full async/await operations through extension methods (no casting required).
/// </summary>
public class Feature14_ReadOnlyDbSetDemo : FeatureDemoBase
{
    public override int FeatureNumber => 14;
    public override string FeatureName => "ReadOnlyDbSet (Immediate Fail-Fast Validation)";

    public Feature14_ReadOnlyDbSetDemo(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override async Task ExecuteFeatureAsync(Guid userId)
    {
        var contextFactory = GetService<IDbContextFactory<SampleDbContext, Guid>>();

        LogDebug("READONLY-DBSET", "ReadOnlyDbSet provides fail-fast protection for read-only entities");
        LogDebug("READONLY-DBSET", "Extension methods enable async operations without casting");

        // Demonstrate async query operations - NO CASTING REQUIRED
        await using (var context = contextFactory.Create())
        {
            var readOnlySet = context.Set<LegacySystem>().AsReadOnly<LegacySystem, int>();

            LogFeature("READONLY-DBSET", "✓ Async queries work directly via extension methods:");
            
            // ToListAsync - no casting needed
            var systems = await readOnlySet.ToListAsync();
            LogFeature("READONLY-DBSET", "  - ToListAsync(): {Count} systems", systems.Count);

            // CountAsync - no casting needed
            var count = await readOnlySet.CountAsync();
            LogFeature("READONLY-DBSET", "  - CountAsync(): {Count}", count);

            // AnyAsync with predicate - no casting needed
            var hasActive = await readOnlySet.AnyAsync(s => s.SystemName.Contains("Legacy"));
            LogFeature("READONLY-DBSET", "  - AnyAsync(predicate): {HasActive}", hasActive);

            // LINQ composition with async execution - no casting needed
            var filtered = await readOnlySet
                .Where(s => s.SystemName.StartsWith("Legacy"))
                .OrderBy(s => s.SystemName)
                .ToListAsync();
            LogFeature("READONLY-DBSET", "  - LINQ + ToListAsync(): {Count} filtered", filtered.Count);
        }

        // Demonstrate write protection - throws immediately
        await using (var context = contextFactory.Create())
        {
            var readOnlySet = context.Set<LegacySystem>().AsReadOnly<LegacySystem, int>();

            LogFeature("READONLY-DBSET", "✓ Write operations throw immediately (fail-fast):");

            try
            {
                readOnlySet.Add(new LegacySystem { Id = 999, SystemName = "Test" });
                LogFeature("READONLY-DBSET", "  - ❌ Add() should have thrown!");
            }
            catch (ReadOnlyEntityException ex)
            {
                LogFeature("READONLY-DBSET", "  - Add() threw ReadOnlyEntityException ✓");
                LogFeature("READONLY-DBSET", "    Message: {Message}", ex.Message);
            }

            try
            {
                var system = new LegacySystem { Id = 1, SystemName = "Test" };
                readOnlySet.Update(system);
                LogFeature("READONLY-DBSET", "  - ❌ Update() should have thrown!");
            }
            catch (ReadOnlyEntityException ex)
            {
                LogFeature("READONLY-DBSET", "  - Update() threw ReadOnlyEntityException ✓");
            }

            try
            {
                var system = new LegacySystem { Id = 1, SystemName = "Test" };
                readOnlySet.Remove(system);
                LogFeature("READONLY-DBSET", "  - ❌ Remove() should have thrown!");
            }
            catch (ReadOnlyEntityException ex)
            {
                LogFeature("READONLY-DBSET", "  - Remove() threw ReadOnlyEntityException ✓");
            }
        }

        LogFeature("READONLY-DBSET", "✓ ReadOnlyDbSet provides fail-fast protection");
        LogFeature("READONLY-DBSET", "✓ Extension methods eliminate casting requirements");
        LogFeature("READONLY-DBSET", "✓ Full async/await support for all query operations");
    }
}

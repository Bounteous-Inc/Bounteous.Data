using Bounteous.Data.Extensions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates the EnforceReadOnly feature for query-only request protection.
/// Shows how to use context.EnforceReadOnly() to prevent accidental data modifications
/// in GET endpoints, reports, and other read-only operations.
/// </summary>
public class Feature15_EnforceReadOnlyDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 15;
    public string FeatureName => "EnforceReadOnly (Query-Only Request Protection)";

    public Feature15_EnforceReadOnlyDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Debug("[ENFORCE-READONLY] Use context.EnforceReadOnly() for query-only operations");
        Log.Debug("[ENFORCE-READONLY] Perfect for GET endpoints, reports, and list views");

        var contextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SampleDbContext, Guid>>();

        // Demonstrate query-only operation (simulating a GET endpoint)
        await DemonstrateQueryOnlyOperation(contextFactory, userId);

        // Demonstrate report generation (another query-only scenario)
        await DemonstrateReportGeneration(contextFactory, userId);

        // Demonstrate what happens if code accidentally tries to modify data
        await DemonstrateAccidentalModificationProtection(contextFactory, userId);

        Log.Information("[ENFORCE-READONLY] ✓ EnforceReadOnly provides request-level protection");
        Log.Information("[ENFORCE-READONLY] ✓ Perfect for GET endpoints and query-only operations");
        Log.Information("[ENFORCE-READONLY] ✓ Discoverable via IntelliSense on context");
    }

    private async Task DemonstrateQueryOnlyOperation(IDbContextFactory<SampleDbContext, Guid> contextFactory, Guid userId)
    {
        Log.Debug("[ENFORCE-READONLY] Simulating GET /api/customers endpoint");
        using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
        {
            // Enforce read-only mode - any SaveChanges will throw
            using var scope = context.EnforceReadOnly();

            var customers = await context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Take(10)
                .ToListAsync();

            Log.Information("[ENFORCE-READONLY] ✓ Query operations work: Found {Count} customer(s)", customers.Count);
            Log.Information("[ENFORCE-READONLY] ✓ Scope ensures no accidental data modifications");
        }
    }

    private async Task DemonstrateReportGeneration(IDbContextFactory<SampleDbContext, Guid> contextFactory, Guid userId)
    {
        Log.Debug("[ENFORCE-READONLY] Simulating report generation");
        using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
        {
            using var scope = context.EnforceReadOnly();

            var report = await context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.Status == OrderStatus.Delivered)
                .Select(o => new
                {
                    OrderId = o.Id,
                    CustomerName = o.Customer.Name,
                    ItemCount = o.OrderItems.Count,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            Log.Information("[ENFORCE-READONLY] ✓ Report generated: {Count} delivered order(s)", report.Count);
            Log.Information("[ENFORCE-READONLY] ✓ Complex queries with navigation properties work");
        }
    }

    private async Task DemonstrateAccidentalModificationProtection(IDbContextFactory<SampleDbContext, Guid> contextFactory, Guid userId)
    {
        Log.Debug("[ENFORCE-READONLY] Testing accidental modification (should throw)");
        try
        {
            using var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId);
            using var scope = context.EnforceReadOnly();

            var customer = await context.Customers.FirstOrDefaultAsync();
            if (customer != null)
            {
                // Developer accidentally modifies data (bug in code)
                customer.Name = "Accidentally Modified";

                // This will throw InvalidOperationException
                await context.SaveChangesAsync();
                Log.Warning("[ENFORCE-READONLY] ✗ UNEXPECTED: SaveChanges did not throw");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("read-only request scope"))
        {
            Log.Information("[ENFORCE-READONLY] ✓ SaveChanges threw InvalidOperationException");
            Log.Information("[ENFORCE-READONLY]   - Message: {Message}", ex.Message.Split('\n')[0]);
            Log.Information("[ENFORCE-READONLY]   - Protection: Accidental modifications blocked");
        }
    }
}

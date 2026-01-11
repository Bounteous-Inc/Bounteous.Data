using Bounteous.Data.Sample.Services;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates automatic auditing with AuditVisitor.AcceptNew.
/// Shows how CreatedBy, CreatedOn, and Version are automatically set on new entities.
/// </summary>
public class Feature01_AutomaticAuditingDemo : FeatureDemoBase
{
    public override int FeatureNumber => 1;
    public override string FeatureName => "Automatic Auditing (CreatedBy, CreatedOn, Version)";

    public Feature01_AutomaticAuditingDemo(IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
    }

    protected override async Task ExecuteFeatureAsync(Guid userId)
    {
        LogDebug("AUDIT", "AuditVisitor.AcceptNew will be called for new entities");

        var customerService = GetService<ICustomerService>();

        var customer1 = await customerService.CreateCustomerAsync(
            "John Doe",
            "john.doe@example.com",
            "555-1234",
            userId);

        LogFeature("AUDIT", "✓ Customer Created: {Name}", customer1.Name);
        LogFeature("AUDIT", "  - CreatedBy: {CreatedBy}", customer1.CreatedBy);
        LogFeature("AUDIT", "  - CreatedOn: {CreatedOn:yyyy-MM-dd HH:mm:ss}", customer1.CreatedOn);
        LogFeature("AUDIT", "  - Version: {Version}", customer1.Version);

        var customer2 = await customerService.CreateCustomerAsync(
            "Jane Smith",
            "jane.smith@example.com",
            "555-5678",
            userId);

        LogFeature("AUDIT", "✓ Customer Created: {Name} (CreatedBy: {CreatedBy})", customer2.Name, customer2.CreatedBy);
    }
}

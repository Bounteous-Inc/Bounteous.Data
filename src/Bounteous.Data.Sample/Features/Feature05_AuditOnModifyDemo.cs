using Bounteous.Data.Sample.Services;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates audit on modify with version increment.
/// Shows how ModifiedBy, ModifiedOn, and Version are automatically updated.
/// </summary>
public class Feature05_AuditOnModifyDemo : FeatureDemoBase
{
    public override int FeatureNumber => 5;
    public override string FeatureName => "Audit on Modify (ModifiedBy, ModifiedOn, Version++)";

    public Feature05_AuditOnModifyDemo(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override async Task ExecuteFeatureAsync(Guid userId)
    {
        LogDebug("AUDIT-MODIFY", "AuditVisitor.AcceptModified will increment version");

        var productService = GetService<IProductService>();
        
        // Create a product first
        var product = await productService.CreateProductAsync("Test Product", "For modification demo", 999.99m, 5, "TEST-001", userId);
        LogFeature("AUDIT-MODIFY", "Created product with Version: {Version}", product.Version);
        
        // Now update it to demonstrate audit on modify
        var updatedProduct = await productService.UpdateProductPriceAsync(product.Id, 1399.99m, userId);
        LogFeature("AUDIT-MODIFY", "✓ Product Updated: {Name}", updatedProduct.Name);
        LogFeature("AUDIT-MODIFY", "  - ModifiedBy: {ModifiedBy}", updatedProduct.ModifiedBy);
        LogFeature("AUDIT-MODIFY", "  - ModifiedOn: {ModifiedOn:yyyy-MM-dd HH:mm:ss}", updatedProduct.ModifiedOn);
        LogFeature("AUDIT-MODIFY", "  - Version: {Version} (incremented from 1)", updatedProduct.Version);
    }
}

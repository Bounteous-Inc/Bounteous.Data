using Bounteous.Data.Extensions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Sample.Services;

public class ProductService : IProductService
{
    private readonly IDbContextFactory<SampleDbContext> _contextFactory;

    public ProductService(IDbContextFactory<SampleDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Product> CreateProductAsync(string name, string? description, decimal price, int stockQuantity, string? sku, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            Sku = sku
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> GetProductAsync(Guid productId)
    {
        using var context = _contextFactory.Create();
        return await context.Products.FindById(productId);
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        using var context = _contextFactory.Create();
        
        return await context.Products
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product> UpdateProductPriceAsync(Guid productId, decimal newPrice, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var product = await context.Products.FindById(productId);
        if (product == null)
            throw new InvalidOperationException($"Product {productId} not found");

        product.Price = newPrice;
        await context.SaveChangesAsync();

        return product;
    }

    public async Task DeleteProductAsync(Guid productId, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var product = await context.Products.FindById(productId);
        if (product == null)
            throw new InvalidOperationException($"Product {productId} not found");

        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }
}

using Bounteous.Data.Sample.Domain.Entities;

namespace Bounteous.Data.Sample.Services;

public interface IProductService
{
    Task<Product> CreateProductAsync(string name, string? description, decimal price, int stockQuantity, string? sku, Guid userId);
    Task<Product?> GetProductAsync(Guid productId);
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> UpdateProductPriceAsync(Guid productId, decimal newPrice, Guid userId);
    Task DeleteProductAsync(Guid productId, Guid userId);
}

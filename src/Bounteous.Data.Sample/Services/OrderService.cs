using Bounteous.Data.Extensions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Sample.Services;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<SampleDbContext, Guid> _contextFactory;

    public OrderService(IDbContextFactory<SampleDbContext, Guid> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Order> CreateOrderAsync(Guid customerId, List<(Guid productId, int quantity)> items, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var customer = await context.Customers.FindById(customerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer {customerId} not found");

        var productIds = items.Select(i => i.productId).ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("One or more products not found");

        var order = new Order
        {
            CustomerId = customerId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        var orderItems = new List<OrderItem>();
        foreach (var (productId, quantity) in items)
        {
            var product = products.First(p => p.Id == productId);
            
            if (product.StockQuantity < quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");

            var orderItem = new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };
            
            orderItems.Add(orderItem);
            product.StockQuantity -= quantity;
        }

        order.OrderItems = orderItems;
        order.TotalAmount = orderItems.Sum(oi => oi.TotalPrice);

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order;
    }

    public async Task<Order?> GetOrderAsync(Guid orderId)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(Guid customerId)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CustomerId == customerId && !o.IsDeleted)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);
        
        var order = await context.Orders.FindById(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        order.Status = status;
        await context.SaveChangesAsync();

        return order;
    }

    public async Task DeleteOrderAsync(Guid orderId, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);
        
        var order = await context.Orders.FindById(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        context.Orders.Remove(order);
        await context.SaveChangesAsync();
    }
}

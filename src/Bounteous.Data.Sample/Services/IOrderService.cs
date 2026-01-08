using Bounteous.Data.Sample.Domain;

namespace Bounteous.Data.Sample.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid customerId, List<(Guid productId, int quantity)> items, Guid userId);
    Task<Order?> GetOrderAsync(Guid orderId);
    Task<List<Order>> GetCustomerOrdersAsync(Guid customerId);
    Task<Order> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid userId);
    Task DeleteOrderAsync(Guid orderId, Guid userId);
}

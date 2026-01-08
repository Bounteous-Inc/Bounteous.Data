using Bounteous.Data.Sample.Domain;

namespace Bounteous.Data.Sample.Services;

public interface ICustomerService
{
    Task<Customer> CreateCustomerAsync(string name, string email, string? phoneNumber, Guid userId);
    Task<Customer?> GetCustomerAsync(Guid customerId);
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer> UpdateCustomerAsync(Guid customerId, string name, string email, string? phoneNumber, Guid userId);
    Task DeleteCustomerAsync(Guid customerId, Guid userId);
}

using Bounteous.Data.Extensions;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Sample.Services;

public class CustomerService : ICustomerService
{
    private readonly IDbContextFactory<SampleDbContext, Guid> _contextFactory;

    public CustomerService(IDbContextFactory<SampleDbContext, Guid> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, string? phoneNumber, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var customer = new Customer
        {
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return customer;
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId)
    {
        using var context = _contextFactory.Create();
        return await context.Customers.FindById(customerId);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        using var context = _contextFactory.Create();
        
        return await context.Customers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer> UpdateCustomerAsync(Guid customerId, string name, string email, string? phoneNumber, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var customer = await context.Customers.FindById(customerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer {customerId} not found");

        customer.Name = name;
        customer.Email = email;
        customer.PhoneNumber = phoneNumber;

        await context.SaveChangesAsync();

        return customer;
    }

    public async Task DeleteCustomerAsync(Guid customerId, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserIdTyped(userId);

        var customer = await context.Customers.FindById(customerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer {customerId} not found");

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
    }
}

using Microsoft.Extensions.Configuration;

namespace Bounteous.Data.Sample.Infrastructure;

public class SampleConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public SampleConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ConnectionString => _configuration.GetConnectionString("DefaultConnection") 
        ?? "InMemory";
}

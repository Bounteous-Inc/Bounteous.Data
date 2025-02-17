using System.Data;

namespace Bounteous.Data;

public interface IConnectionBuilder
{
    Task<IDbConnection> CreateConnectionAsync();
    Task<IDbConnection> CreateReadConnectionAsync();
    string AdminConnectionString { get; }
}

public class ConnectionBuilder(IConnectionStringProvider connectionStringProvider) : IConnectionBuilder
{
    public Task<IDbConnection> CreateConnectionAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IDbConnection> CreateReadConnectionAsync()
    {
        throw new NotImplementedException();
    }

    public string AdminConnectionString => connectionStringProvider.ConnectionString;
}
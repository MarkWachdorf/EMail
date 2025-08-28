using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Email.Infrastructure.DependencyInjection;

namespace Email.Infrastructure.Connection;

/// <summary>
/// SQL Server implementation of <see cref="IDbConnectionFactory"/> using Microsoft.Data.SqlClient.
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly InfrastructureOptions _options;

    public SqlConnectionFactory(IConfiguration configuration, IOptions<InfrastructureOptions>? options = null)
    {
        _options = options?.Value ?? new InfrastructureOptions();
        
        _connectionString = configuration.GetConnectionString(_options.ConnectionStringName) 
                            ?? throw new ArgumentNullException(nameof(configuration), 
                                $"Connection string '{_options.ConnectionStringName}' is not configured.");
    }

    /// <summary>
    /// Creates and opens a new SqlConnection.
    /// </summary>
    /// <returns>An opened SqlConnection instance.</returns>
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    /// Gets the connection string (for testing purposes).
    /// </summary>
    /// <returns>The connection string.</returns>
    internal string GetConnectionString() => _connectionString;
}

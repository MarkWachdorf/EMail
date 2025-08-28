using System.Data;

namespace Email.Infrastructure.Connection;

/// <summary>
/// Defines a factory for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    /// <returns>An opened IDbConnection instance.</returns>
    IDbConnection CreateConnection();
}

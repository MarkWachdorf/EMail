namespace Email.Infrastructure.DependencyInjection;

/// <summary>
/// Configuration options for infrastructure services.
/// </summary>
public class InfrastructureOptions
{
    /// <summary>
    /// The name of the connection string in configuration.
    /// </summary>
    public string ConnectionStringName { get; set; } = "DefaultConnection";

    /// <summary>
    /// Whether to enable retry policy for database operations.
    /// </summary>
    public bool EnableRetryPolicy { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 1;

    /// <summary>
    /// Whether to enable connection pooling.
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Maximum pool size for connection pooling.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Minimum pool size for connection pooling.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Command timeout in seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
}

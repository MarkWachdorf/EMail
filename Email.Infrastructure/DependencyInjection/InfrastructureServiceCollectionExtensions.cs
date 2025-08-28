using Email.Infrastructure.Connection;
using Email.Infrastructure.Repositories;
using Email.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering infrastructure services in the DI container.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register connection factory
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        // Register repositories
        services.AddRepositories();

        return services;
    }

    /// <summary>
    /// Adds infrastructure services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureOptions">Action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        Action<InfrastructureOptions> configureOptions)
    {
        // Configure options
        var options = new InfrastructureOptions();
        configureOptions(options);
        services.Configure<InfrastructureOptions>(opt =>
        {
            opt.ConnectionStringName = options.ConnectionStringName;
            opt.EnableRetryPolicy = options.EnableRetryPolicy;
            opt.MaxRetryAttempts = options.MaxRetryAttempts;
            opt.RetryDelaySeconds = options.RetryDelaySeconds;
            opt.EnableConnectionPooling = options.EnableConnectionPooling;
            opt.MaxPoolSize = options.MaxPoolSize;
            opt.MinPoolSize = options.MinPoolSize;
            opt.ConnectionTimeout = options.ConnectionTimeout;
            opt.CommandTimeout = options.CommandTimeout;
        });

        // Register connection factory
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        // Register repositories
        services.AddRepositories();

        return services;
    }

    /// <summary>
    /// Adds infrastructure services to the service collection with custom configuration and repository lifetime.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureOptions">Action to configure additional options.</param>
    /// <param name="repositoryLifetime">The lifetime for repositories.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        Action<InfrastructureOptions> configureOptions,
        ServiceLifetime repositoryLifetime)
    {
        // Configure options
        var options = new InfrastructureOptions();
        configureOptions(options);
        services.Configure<InfrastructureOptions>(opt =>
        {
            opt.ConnectionStringName = options.ConnectionStringName;
            opt.EnableRetryPolicy = options.EnableRetryPolicy;
            opt.MaxRetryAttempts = options.MaxRetryAttempts;
            opt.RetryDelaySeconds = options.RetryDelaySeconds;
            opt.EnableConnectionPooling = options.EnableConnectionPooling;
            opt.MaxPoolSize = options.MaxPoolSize;
            opt.MinPoolSize = options.MinPoolSize;
            opt.ConnectionTimeout = options.ConnectionTimeout;
            opt.CommandTimeout = options.CommandTimeout;
        });

        // Register connection factory
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        // Register repositories with custom lifetime
        services.AddRepositories(repositoryLifetime);

        return services;
    }
}

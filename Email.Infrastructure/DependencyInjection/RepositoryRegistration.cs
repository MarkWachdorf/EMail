using Email.Infrastructure.Repositories;
using Email.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Infrastructure.DependencyInjection;

/// <summary>
/// Static class for registering repositories in the DI container.
/// </summary>
public static class RepositoryRegistration
{
    /// <summary>
    /// Registers all repositories in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register repositories with scoped lifetime
        services.AddScoped<IEmailMessageRepository, EmailMessageRepository>();
        services.AddScoped<IEmailCacheRepository, EmailCacheRepository>();
        services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
        services.AddScoped<IEmailHistoryRepository, EmailHistoryRepository>();

        return services;
    }

    /// <summary>
    /// Registers repositories with custom lifetime.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services, ServiceLifetime lifetime)
    {
        // Register repositories with specified lifetime
        services.Add(new ServiceDescriptor(typeof(IEmailMessageRepository), typeof(EmailMessageRepository), lifetime));
        services.Add(new ServiceDescriptor(typeof(IEmailCacheRepository), typeof(EmailCacheRepository), lifetime));
        services.Add(new ServiceDescriptor(typeof(IErrorLogRepository), typeof(ErrorLogRepository), lifetime));
        services.Add(new ServiceDescriptor(typeof(IEmailHistoryRepository), typeof(EmailHistoryRepository), lifetime));

        return services;
    }
}

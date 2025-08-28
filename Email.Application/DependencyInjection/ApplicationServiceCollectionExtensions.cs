using Email.Application.Services;
using Email.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Application.DependencyInjection;

/// <summary>
/// Extension methods for registering application services in the DI container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services (business logic layer) to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services with appropriate lifetimes
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<IEmailCacheService, EmailCacheService>();
        services.AddScoped<IErrorLogService, ErrorLogService>();
        
        // Register email sending services
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailConfiguration, EmailConfiguration>();

        return services;
    }
}

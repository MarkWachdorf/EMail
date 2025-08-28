using Email.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Email.Application.Services;

/// <summary>
/// Implementation of email configuration that reads settings from IConfiguration.
/// </summary>
public class EmailConfiguration : IEmailConfiguration
{
    private readonly IConfiguration _configuration;

    public EmailConfiguration(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string SmtpHost => _configuration["Email:SmtpHost"] ?? "localhost";

    public int SmtpPort => int.TryParse(_configuration["Email:SmtpPort"], out int port) ? port : 587;

    public string SmtpUsername => _configuration["Email:SmtpUsername"] ?? string.Empty;

    public string SmtpPassword => _configuration["Email:SmtpPassword"] ?? string.Empty;

    public bool UseSsl => bool.TryParse(_configuration["Email:UseSsl"], out bool useSsl) && useSsl;

    public bool UseStartTls => bool.TryParse(_configuration["Email:UseStartTls"], out bool useStartTls) && useStartTls;

    public string DefaultFromAddress => _configuration["Email:DefaultFromAddress"] ?? "noreply@example.com";

    public string DefaultFromName => _configuration["Email:DefaultFromName"] ?? "Email Service";

    public int ConnectionTimeout => int.TryParse(_configuration["Email:ConnectionTimeout"], out int timeout) ? timeout : 30;

    public bool RequireAuthentication => bool.TryParse(_configuration["Email:RequireAuthentication"], out bool requireAuth) && requireAuth;
}

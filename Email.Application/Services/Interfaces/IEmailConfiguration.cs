namespace Email.Application.Services.Interfaces;

/// <summary>
/// Configuration settings for email sending via SMTP.
/// </summary>
public interface IEmailConfiguration
{
    /// <summary>
    /// SMTP server hostname or IP address.
    /// </summary>
    string SmtpHost { get; }

    /// <summary>
    /// SMTP server port number.
    /// </summary>
    int SmtpPort { get; }

    /// <summary>
    /// Username for SMTP authentication.
    /// </summary>
    string SmtpUsername { get; }

    /// <summary>
    /// Password for SMTP authentication.
    /// </summary>
    string SmtpPassword { get; }

    /// <summary>
    /// Whether to use SSL/TLS for SMTP connection.
    /// </summary>
    bool UseSsl { get; }

    /// <summary>
    /// Whether to use STARTTLS for SMTP connection.
    /// </summary>
    bool UseStartTls { get; }

    /// <summary>
    /// Default sender email address to use when none is specified.
    /// </summary>
    string DefaultFromAddress { get; }

    /// <summary>
    /// Default sender display name to use when none is specified.
    /// </summary>
    string DefaultFromName { get; }

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    int ConnectionTimeout { get; }

    /// <summary>
    /// Whether to require authentication.
    /// </summary>
    bool RequireAuthentication { get; }
}

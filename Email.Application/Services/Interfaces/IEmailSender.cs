using Email.Infrastructure.Entities;

namespace Email.Application.Services.Interfaces;

/// <summary>
/// Interface for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="emailMessage">The email message to send.</param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    Task<bool> SendEmailAsync(EmailMessage emailMessage);
}

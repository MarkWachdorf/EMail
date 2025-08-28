using Email.Application.Services.Interfaces;
using Email.Infrastructure.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Email.Application.Services;

/// <summary>
/// Service for sending emails using MailKit and SMTP.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly IEmailConfiguration _emailConfiguration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IEmailConfiguration emailConfiguration, ILogger<EmailSender> logger)
    {
        _emailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(EmailMessage emailMessage)
    {
        try
        {
            _logger.LogInformation("Preparing to send email {EmailId} to {ToAddresses}", 
                emailMessage.Id, emailMessage.ToAddresses);

            // Create the email message
            var message = CreateMimeMessage(emailMessage);

            // Send the email
            using var client = new SmtpClient();
            
            // Connect to the SMTP server
            await ConnectToSmtpServerAsync(client);
            
            // Send the message
            await client.SendAsync(message);
            
            // Disconnect
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email {EmailId} sent successfully to {ToAddresses}", 
                emailMessage.Id, emailMessage.ToAddresses);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email {EmailId} to {ToAddresses}. Error: {ErrorMessage}", 
                emailMessage.Id, emailMessage.ToAddresses, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Creates a MimeMessage from the EmailMessage entity.
    /// </summary>
    /// <param name="emailMessage">The email message entity.</param>
    /// <returns>A MimeMessage ready to be sent.</returns>
    private MimeMessage CreateMimeMessage(EmailMessage emailMessage)
    {
        var message = new MimeMessage();

        // Set the sender
        var fromAddress = !string.IsNullOrEmpty(emailMessage.FromAddress) 
            ? emailMessage.FromAddress 
            : _emailConfiguration.DefaultFromAddress;
        
        var fromName = _emailConfiguration.DefaultFromName;
        message.From.Add(new MailboxAddress(fromName, fromAddress));

        // Set recipients
        AddRecipients(message.To, emailMessage.ToAddresses);
        
        // Set CC recipients if any
        if (!string.IsNullOrEmpty(emailMessage.CcAddresses))
        {
            AddRecipients(message.Cc, emailMessage.CcAddresses);
        }
        
        // Set BCC recipients if any
        if (!string.IsNullOrEmpty(emailMessage.BccAddresses))
        {
            AddRecipients(message.Bcc, emailMessage.BccAddresses);
        }

        // Set subject
        message.Subject = emailMessage.Subject;

        // Set importance
        message.Priority = GetMessagePriority(emailMessage.ImportanceFlag);

        // Create the body
        var bodyBuilder = new BodyBuilder();
        
        // Determine if the body is HTML or plain text
        if (emailMessage.HtmlFlag)
        {
            bodyBuilder.HtmlBody = emailMessage.Body;
        }
        else
        {
            bodyBuilder.TextBody = emailMessage.Body;
        }

        // Add header and footer if present
        var fullBody = BuildFullBody(emailMessage);
        if (emailMessage.HtmlFlag)
        {
            bodyBuilder.HtmlBody = fullBody;
        }
        else
        {
            bodyBuilder.TextBody = fullBody;
        }

        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }

    /// <summary>
    /// Adds recipients to the specified collection.
    /// </summary>
    /// <param name="collection">The recipient collection to add to.</param>
    /// <param name="addresses">Comma or semicolon separated email addresses.</param>
    private void AddRecipients(InternetAddressList collection, string addresses)
    {
        if (string.IsNullOrEmpty(addresses)) return;

        // Split by comma or semicolon
        var addressList = addresses.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var address in addressList)
        {
            var trimmedAddress = address.Trim();
            if (!string.IsNullOrEmpty(trimmedAddress))
            {
                collection.Add(MailboxAddress.Parse(trimmedAddress));
            }
        }
    }

    /// <summary>
    /// Gets the message priority based on the importance flag.
    /// </summary>
    /// <param name="importance">The importance flag.</param>
    /// <returns>The message priority.</returns>
    private MessagePriority GetMessagePriority(Email.Infrastructure.Entities.Enums.EmailImportanceFlag importance)
    {
        return importance switch
        {
            Email.Infrastructure.Entities.Enums.EmailImportanceFlag.High => MessagePriority.Urgent,
            Email.Infrastructure.Entities.Enums.EmailImportanceFlag.Low => MessagePriority.NonUrgent,
            _ => MessagePriority.Normal
        };
    }

    /// <summary>
    /// Builds the full email body including header and footer.
    /// </summary>
    /// <param name="emailMessage">The email message entity.</param>
    /// <returns>The complete email body.</returns>
    private string BuildFullBody(EmailMessage emailMessage)
    {
        var body = emailMessage.Body;

        // Add header if present
        if (!string.IsNullOrEmpty(emailMessage.Header))
        {
            if (emailMessage.HtmlFlag)
            {
                body = $"<div style='margin-bottom: 20px;'>{emailMessage.Header}</div>{body}";
            }
            else
            {
                body = $"{emailMessage.Header}\n\n{body}";
            }
        }

        // Add footer if present
        if (!string.IsNullOrEmpty(emailMessage.Footer))
        {
            if (emailMessage.HtmlFlag)
            {
                body = $"{body}<div style='margin-top: 20px; border-top: 1px solid #ccc; padding-top: 10px;'>{emailMessage.Footer}</div>";
            }
            else
            {
                body = $"{body}\n\n{emailMessage.Footer}";
            }
        }

        return body;
    }

    /// <summary>
    /// Connects to the SMTP server with the configured settings.
    /// </summary>
    /// <param name="client">The SMTP client.</param>
    private async Task ConnectToSmtpServerAsync(SmtpClient client)
    {
        var options = SecureSocketOptions.None;
        
        if (_emailConfiguration.UseSsl)
        {
            options = SecureSocketOptions.SslOnConnect;
        }
        else if (_emailConfiguration.UseStartTls)
        {
            options = SecureSocketOptions.StartTls;
        }

        // Connect to the server
        await client.ConnectAsync(_emailConfiguration.SmtpHost, _emailConfiguration.SmtpPort, options);

        // Authenticate if required
        if (_emailConfiguration.RequireAuthentication && 
            !string.IsNullOrEmpty(_emailConfiguration.SmtpUsername) && 
            !string.IsNullOrEmpty(_emailConfiguration.SmtpPassword))
        {
            await client.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
        }
    }
}

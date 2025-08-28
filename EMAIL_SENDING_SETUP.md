# Email Sending Setup with MailKit

This document explains how to set up and use the email sending functionality that has been implemented using MailKit and MimeKit.

## Overview

The email sending functionality has been integrated into the `EmailService.CreateEmailAsync` method. When an email is created, it will now attempt to send the email immediately using the configured SMTP server.

## Configuration

### SMTP Settings

Add the following configuration to your `appsettings.json` or `appsettings.Development.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "UseSsl": false,
    "UseStartTls": true,
    "DefaultFromAddress": "noreply@yourcompany.com",
    "DefaultFromName": "Your Company Email Service",
    "ConnectionTimeout": 30,
    "RequireAuthentication": true
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `SmtpHost` | SMTP server hostname or IP address | `localhost` |
| `SmtpPort` | SMTP server port number | `587` |
| `SmtpUsername` | Username for SMTP authentication | Empty string |
| `SmtpPassword` | Password for SMTP authentication | Empty string |
| `UseSsl` | Whether to use SSL/TLS for SMTP connection | `false` |
| `UseStartTls` | Whether to use STARTTLS for SMTP connection | `false` |
| `DefaultFromAddress` | Default sender email address | `noreply@example.com` |
| `DefaultFromName` | Default sender display name | `Email Service` |
| `ConnectionTimeout` | Connection timeout in seconds | `30` |
| `RequireAuthentication` | Whether to require authentication | `false` |

## Common SMTP Providers

### Gmail
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "RequireAuthentication": true
  }
}
```

### Outlook/Hotmail
```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "RequireAuthentication": true
  }
}
```

### Office 365
```json
{
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "RequireAuthentication": true
  }
}
```

### SendGrid
```json
{
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "UseStartTls": true,
    "RequireAuthentication": true
  }
}
```

## Security Considerations

1. **App Passwords**: For Gmail and other providers that support 2FA, use app passwords instead of your regular password.

2. **Environment Variables**: Store sensitive information like passwords in environment variables or Azure Key Vault:
   ```json
   {
     "Email": {
       "SmtpPassword": "%EMAIL_SMTP_PASSWORD%"
     }
   }
   ```

3. **Connection Security**: Always use SSL/TLS or STARTTLS for production environments.

## Usage

The email sending is now automatically triggered when you call `EmailService.CreateEmailAsync()`. The service will:

1. Create the email record in the database
2. Attempt to send the email immediately
3. Update the email status based on the sending result
4. Log the attempt in the email history
5. Log any errors in the error log

### Example

```csharp
var request = new CreateEmailRequest
{
    CompanyCode = "COMPANY",
    ApplicationCode = "APP",
    FromAddress = "sender@example.com",
    ToAddresses = "recipient@example.com",
    Subject = "Test Email",
    Body = "This is a test email",
    ImportanceFlag = EmailImportanceDto.Normal,
    MaxRetries = 3
};

var result = await _emailService.CreateEmailAsync(request);
```

## Error Handling

The system includes comprehensive error handling:

- **Retry Logic**: Failed emails will be retried up to the configured `MaxRetries` count
- **Status Tracking**: Email status is updated to `Sent`, `Failed`, or `Pending` based on the result
- **Error Logging**: All errors are logged to the database for monitoring
- **History Tracking**: All sending attempts are recorded in the email history

## Testing

For testing purposes, you can:

1. Use a test SMTP server like MailHog or Papercut
2. Configure the SMTP settings to point to your test server
3. Use the existing unit tests which mock the email sender

## Troubleshooting

### Common Issues

1. **Authentication Failed**: Check your username/password and ensure you're using app passwords if required
2. **Connection Timeout**: Verify the SMTP host and port are correct
3. **SSL/TLS Issues**: Ensure the correct security settings are configured for your provider
4. **Port Blocked**: Some networks block SMTP ports; try using port 587 with STARTTLS

### Logging

Check the application logs for detailed error messages. The email sender logs:
- Connection attempts
- Authentication results
- Sending success/failure
- Detailed error messages

## Dependencies

The implementation uses:
- **MailKit**: For SMTP client functionality
- **MimeKit**: For email message creation and parsing

These packages are automatically included when you add the MailKit NuGet package.

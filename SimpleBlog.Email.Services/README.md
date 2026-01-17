# SimpleBlog.Email.Services

## Overview

Email service library providing SMTP-based email sending capabilities for SimpleBlog notifications and user communications.

## Technologies

- **.NET 9.0** - Framework
- **MailKit** - SMTP client library
- **MimeKit** - Email message construction

## Project Structure

```
SimpleBlog.Email.Services/
├── SmtpEmailService.cs       # SMTP implementation
└── GlobalUsings.cs           # Global using directives
```

## Key Components

### IEmailService Interface

```csharp
public interface IEmailService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken ct = default);
    
    Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken ct = default);
}
```

### SmtpEmailService

```csharp
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    
    public SmtpEmailService(
        IOptions<SmtpSettings> settings,
        ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }
    
    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        
        var bodyBuilder = new BodyBuilder();
        if (isHtml)
        {
            bodyBuilder.HtmlBody = body;
        }
        else
        {
            bodyBuilder.TextBody = body;
        }
        message.Body = bodyBuilder.ToMessageBody();
        
        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, ct);
        
        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        }
        
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
        
        _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
    }
}
```

## Configuration

### SMTP Settings

```csharp
public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "SimpleBlog";
}
```

### appsettings.json

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@simpleblog.com",
    "FromName": "SimpleBlog"
  }
}
```

## Usage

### Service Registration

```csharp
// In Program.cs
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddScoped<IEmailService, SmtpEmailService>();
```

### Send Email

```csharp
public class OrderService
{
    private readonly IEmailService _emailService;
    
    public OrderService(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task ProcessOrderAsync(Order order)
    {
        // Process order...
        
        // Send confirmation email
        await _emailService.SendEmailAsync(
            to: order.Email,
            subject: $"Order #{order.Id} Confirmed",
            body: $"<h1>Thank you for your order!</h1><p>Your order #{order.Id} has been confirmed.</p>",
            isHtml: true);
    }
}
```

## Email Templates

### Order Confirmation

```csharp
public static string GetOrderConfirmationEmail(Order order)
{
    return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .header {{ background: #4CAF50; color: white; padding: 20px; }}
                .content {{ padding: 20px; }}
                .footer {{ background: #f1f1f1; padding: 10px; text-align: center; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>Order Confirmation</h1>
            </div>
            <div class='content'>
                <p>Dear {order.CustomerName},</p>
                <p>Thank you for your order!</p>
                <p><strong>Order Number:</strong> #{order.Id}</p>
                <p><strong>Total:</strong> ${order.Total:F2}</p>
                <p>We will send you another email when your order ships.</p>
            </div>
            <div class='footer'>
                <p>SimpleBlog - {DateTime.UtcNow.Year}</p>
            </div>
        </body>
        </html>";
}
```

### Welcome Email

```csharp
public static string GetWelcomeEmail(string username)
{
    return $@"
        <h1>Welcome to SimpleBlog!</h1>
        <p>Hi {username},</p>
        <p>Thank you for registering with SimpleBlog.</p>
        <p>You can now:</p>
        <ul>
            <li>Read and comment on blog posts</li>
            <li>Shop for products</li>
            <li>Manage your profile</li>
        </ul>
        <p>Best regards,<br/>The SimpleBlog Team</p>";
}
```

## SMTP Providers

### Gmail

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

**Note:** For Gmail, you need to create an [App Password](https://support.google.com/accounts/answer/185833).

### SendGrid

```json
{
  "Smtp": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "UseSsl": true,
    "Username": "apikey",
    "Password": "your-sendgrid-api-key"
  }
}
```

### Mailgun

```json
{
  "Smtp": {
    "Host": "smtp.mailgun.org",
    "Port": 587,
    "UseSsl": true,
    "Username": "postmaster@your-domain.mailgun.org",
    "Password": "your-mailgun-password"
  }
}
```

## Testing

### Development Email Testing

Use services like [Ethereal Email](https://ethereal.email/) for testing:

```csharp
// Generate test account at https://ethereal.email/
{
  "Smtp": {
    "Host": "smtp.ethereal.email",
    "Port": 587,
    "UseSsl": true,
    "Username": "generated-username@ethereal.email",
    "Password": "generated-password"
  }
}
```

### Unit Tests

```csharp
public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_SendsEmail()
    {
        // Arrange
        var settings = Options.Create(new SmtpSettings
        {
            Host = "localhost",
            Port = 25,
            FromEmail = "test@test.com"
        });
        var logger = new NullLogger<SmtpEmailService>();
        var service = new SmtpEmailService(settings, logger);
        
        // Act
        await service.SendEmailAsync(
            "recipient@test.com",
            "Test Subject",
            "Test Body");
        
        // Assert
        // Verify email was sent (requires SMTP test server)
    }
}
```

## Dependencies

- `MailKit` - SMTP client library
- `MimeKit` - Email message construction
- `Microsoft.Extensions.Options` - Configuration
- `Microsoft.Extensions.Logging.Abstractions` - Logging

## Best Practices

1. **Use App Passwords** - Never use main email password for Gmail
2. **Async All The Way** - All email operations should be async
3. **Error Handling** - Catch and log SMTP errors
4. **Retry Logic** - Implement retry for transient failures
5. **Queue Emails** - Use background queue for bulk emails
6. **Template Engine** - Consider Razor templates for complex emails
7. **Test Thoroughly** - Use test SMTP servers in development

## Error Handling

```csharp
public async Task SendEmailAsync(string to, string subject, string body)
{
    try
    {
        // Send email...
    }
    catch (SmtpCommandException ex)
    {
        _logger.LogError(ex, "SMTP command error sending email to {To}", to);
        throw;
    }
    catch (SmtpProtocolException ex)
    {
        _logger.LogError(ex, "SMTP protocol error sending email to {To}", to);
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error sending email to {To}", to);
        throw;
    }
}
```

## Future Enhancements

- [ ] Add email templating engine (Razor, Handlebars)
- [ ] Implement background email queue
- [ ] Add retry logic with Polly
- [ ] Support attachments
- [ ] Add email tracking (opens, clicks)
- [ ] Support CC/BCC recipients
- [ ] Add bulk email sending
- [ ] Implement email scheduling

## Related Documentation

- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [MimeKit Documentation](https://github.com/jstedfast/MimeKit)
- [Email Best Practices](https://sendgrid.com/blog/email-best-practices/)

## Troubleshooting

### Authentication Failed

1. Verify username/password are correct
2. Check if 2FA is enabled (use app password)
3. Ensure "Less secure apps" is enabled (Gmail)

### Connection Timeout

1. Verify SMTP host and port
2. Check firewall rules
3. Verify SSL/TLS settings

### Emails Going to Spam

1. Configure SPF, DKIM, DMARC records
2. Use reputable SMTP provider
3. Avoid spam trigger words
4. Include unsubscribe link

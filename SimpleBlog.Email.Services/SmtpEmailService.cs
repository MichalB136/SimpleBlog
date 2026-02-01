using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleBlog.Common;

namespace SimpleBlog.Email.Services;

public sealed class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly string _smtpServer = configuration["Email:SmtpServer"] ?? "localhost";
    private readonly int _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
    private readonly string _senderEmail = configuration["Email:SenderEmail"] ?? "noreply@simpleblog.local";
    private readonly string _senderName = configuration["Email:SenderName"] ?? "SimpleBlog";
    private readonly string _senderPassword = configuration["Email:SenderPassword"] ?? "";
    private readonly bool _useSsl = bool.Parse(configuration["Email:UseSsl"] ?? "false");

    public async Task SendOrderConfirmationAsync(string customerEmail, string customerName, Order order)
    {
        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new NetworkCredential(_senderEmail, _senderPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = $"Potwierdzenie zamówienia #{order.Id.ToString().Substring(0, 8)}",
                Body = BuildEmailBody(customerName, order),
                IsBodyHtml = true
            };

            mailMessage.To.Add(customerEmail);

            await client.SendMailAsync(mailMessage);
            var maskedEmail = MaskEmail(customerEmail);
            logger.LogInformation("Order confirmation email sent to {Email} for order {OrderId}", maskedEmail, order.Id);
        }
        catch (Exception ex)
        {
            var maskedEmail = MaskEmail(customerEmail);
            logger.LogError(ex, "Failed to send order confirmation email to {Email} for order {OrderId}", maskedEmail, order.Id);
        }
    }

    private static string BuildEmailBody(string customerName, Order order)
    {
        var itemsHtml = string.Join("\n", order.Items.Select(item =>
            $"<tr><td>{item.ProductName}</td><td style=\"text-align:right;\">{item.Quantity}</td><td style=\"text-align:right;\">{item.Price:F2} PLN</td><td style=\"text-align:right;\">{(item.Price * item.Quantity):F2} PLN</td></tr>"
        ));

        return $@"
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-bottom: 3px solid #0d6efd; }}
        .content {{ padding: 20px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th {{ background-color: #0d6efd; color: white; padding: 10px; text-align: left; }}
        td {{ padding: 10px; border-bottom: 1px solid #dee2e6; }}
        .total {{ font-weight: bold; font-size: 18px; text-align: right; padding: 20px 10px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>SimpleBlog</h1>
            <p>Potwierdzenie zamówienia</p>
        </div>
        
        <div class='content'>
            <p>Cześć <strong>{customerName}</strong>,</p>
            
            <p>Dziękujemy za złożenie zamówienia! Oto szczegóły Twojego zamówienia:</p>
            
            <p><strong>Numer zamówienia:</strong> {order.Id.ToString().Substring(0, 8)}</p>
            <p><strong>Data:</strong> {order.CreatedAt:g}</p>
            
            <h3>Produkty:</h3>
            <table>
                <tr>
                    <th>Nazwa produktu</th>
                    <th style='text-align:right;'>Ilość</th>
                    <th style='text-align:right;'>Cena</th>
                    <th style='text-align:right;'>Razem</th>
                </tr>
                {itemsHtml}
            </table>
            
            <div class='total'>
                Suma: {order.TotalAmount:F2} PLN
            </div>
            
            <h3>Adres dostawy:</h3>
            <p>
                {order.CustomerName}<br>
                {order.ShippingAddress}<br>
                {order.ShippingPostalCode} {order.ShippingCity}
            </p>
            
            <p><strong>Dane kontaktowe:</strong></p>
            <p>
                Email: {order.CustomerEmail}<br>
                Telefon: {order.CustomerPhone}
            </p>
            
            <p>Nasz zespół skontaktuje się z Tobą wkrótce w celu potwierdzenia wysyłki.</p>
            
            <p>Dziękujemy za zakupy na SimpleBlog!</p>
        </div>
        
        <div class='footer'>
            <p>&copy; 2024 SimpleBlog. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>
";
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new NetworkCredential(_senderEmail, _senderPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = "Resetowanie hasła - SimpleBlog",
                Body = BuildPasswordResetEmailBody(resetLink),
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            var maskedEmail = MaskEmail(email);
            logger.LogInformation("Password reset email sent to {Email}", maskedEmail);
        }
        catch (Exception ex)
        {
            var maskedEmail = MaskEmail(email);
            logger.LogError(ex, "Failed to send password reset email to {Email}", maskedEmail);
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new NetworkCredential(_senderEmail, _senderPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = "Potwierdź swój adres e-mail - SimpleBlog",
                Body = BuildEmailConfirmationBody(confirmationLink),
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            var maskedEmail = MaskEmail(email);
            logger.LogInformation("Email confirmation sent to {Email}", maskedEmail);
        }
        catch (Exception ex)
        {
            var maskedEmail = MaskEmail(email);
            logger.LogError(ex, "Failed to send email confirmation to {Email}", maskedEmail);
        }
    }

    private static string BuildPasswordResetEmailBody(string resetLink)
    {
        return $@"
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-bottom: 3px solid #0d6efd; }}
        .content {{ padding: 20px; }}
        .button {{ display: inline-block; background-color: #0d6efd; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffecb5; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>SimpleBlog</h1>
            <p>Resetowanie hasła</p>
        </div>
        
        <div class='content'>
            <p>Cześć,</p>
            
            <p>Otrzymaliśmy prośbę o zmianę hasła do Twojego konta SimpleBlog. Kliknij poniższy przycisk, aby zadbować hasło:</p>
            
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Zmień hasło</a>
            </p>
            
            <p>Lub skopiuj i wklej poniższy link do przeglądarki:</p>
            <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 5px;'>{resetLink}</p>
            
            <div class='warning'>
                <p><strong>Ważne:</strong> Ten link wygaśnie za 3 godziny. Jeśli nie prosiłeś o zmianę hasła, możesz bezpiecznie zignorować ten e-mail.</p>
            </div>
            
            <p>Jeśli masz problemy, skontaktuj się z nami na: support@simpleblog.local</p>
        </div>
        
        <div class='footer'>
            <p>&copy; 2024 SimpleBlog. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>
";
    }

    private static string BuildEmailConfirmationBody(string confirmationLink)
    {
        return $@"
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-bottom: 3px solid #28a745; }}
        .content {{ padding: 20px; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>SimpleBlog</h1>
            <p>Potwierdzenie adresu e-mail</p>
        </div>
        
        <div class='content'>
            <p>Cześć,</p>
            
            <p>Dziękujemy za rejestrację na SimpleBlog! Aby aktywować swoje konto, potwierdź swój adres e-mail klikając poniższy przycisk:</p>
            
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>Potwierdź adres e-mail</a>
            </p>
            
            <p>Lub skopiuj i wklej poniższy link do przeglądarki:</p>
            <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 5px;'>{confirmationLink}</p>
            
            <p>Jeśli nie rejestrujesz się na SimpleBlog, możesz bezpiecznie zignorować ten e-mail.</p>
            
            <p>Dziękujemy za dołączenie do naszej społeczności!</p>
        </div>
        
        <div class='footer'>
            <p>&copy; 2024 SimpleBlog. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>
";
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "unknown";
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return "unknown";
        }

        var firstChar = email[..1];
        var domain = email[(atIndex + 1)..];
        return $"{firstChar}***@{domain}";
    }
}

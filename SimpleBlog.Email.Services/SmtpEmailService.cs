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
            logger.LogInformation("Order confirmation email sent to {Email} for order {OrderId}", customerEmail, order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order confirmation email to {Email} for order {OrderId}", customerEmail, order.Id);
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
}

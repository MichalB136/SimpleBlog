using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Interfaces;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string customerEmail, string customerName, Order order);
    Task SendPasswordResetEmailAsync(string email, string resetLink);
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
}

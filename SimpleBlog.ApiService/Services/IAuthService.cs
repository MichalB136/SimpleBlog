using System.Security.Claims;

namespace SimpleBlog.ApiService.Services;

public interface IAuthService
{
    string GenerateToken(string username, string role, TimeSpan? expires = null);
    ClaimsPrincipal? ValidateToken(string token);
}

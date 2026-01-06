using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using SimpleBlog.ApiService;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(
        this WebApplication app,
        string jwtIssuer,
        string jwtAudience,
        byte[] jwtKey)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();
        var authConfig = app.Services.GetRequiredService<AuthorizationConfiguration>();

        app.MapPost(endpointConfig.Login, async (
            LoginRequest request,
            IValidator<LoginRequest> validator,
            IUserRepository userRepo,
            IOperationLogger operationLogger,
            ILogger<Program> logger) =>
        {
            // Validate request using FluentValidation
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                operationLogger.LogValidationFailure("Login", request, validationResult.Errors);
                logger.LogWarning("Login attempt with invalid credentials format");
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var user = await userRepo.ValidateUserAsync(request.Username, request.Password);
            if (user == null)
            {
                logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return Results.Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(authConfig.TokenExpirationHours),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256Signature)
            };
            
            logger.LogInformation("Generating JWT token with Issuer: {Issuer}, Audience: {Audience}, Key length: {KeyLength}", jwtIssuer, jwtAudience, jwtKey.Length);
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            logger.LogInformation("Successful login for user: {Username}, Token length: {TokenLength}", user.Username, tokenString.Length);
            return Results.Ok(new { token = tokenString, username = user.Username, role = user.Role });
        });
    }
}

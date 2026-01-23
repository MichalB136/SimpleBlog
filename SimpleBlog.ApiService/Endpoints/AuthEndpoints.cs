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

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshExpires = DateTime.UtcNow.AddDays(authConfig.RefreshTokenExpirationDays);
            await userRepo.SaveRefreshTokenAsync(user.Username, refreshToken, refreshExpires);

            logger.LogInformation("Successful login for user: {Username}, Token length: {TokenLength}", user.Username, tokenString.Length);
            return Results.Ok(new { token = tokenString, refreshToken, username = user.Username, role = user.Role });
        });

        app.MapPost(endpointConfig.Refresh, async (
            RefreshRequest request,
            IUserRepository userRepo,
            ILogger<Program> logger) =>
        {
            var oldToken = request.RefreshToken;
            var username = await userRepo.GetUsernameByRefreshTokenAsync(oldToken);
            if (username is null)
            {
                logger.LogWarning("Invalid or expired refresh token provided");
                return Results.Unauthorized();
            }

            var user = await userRepo.GetUserByUsernameAsync(username);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            // Create new access token
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

            var newToken = tokenHandler.CreateToken(tokenDescriptor);
            var newTokenString = tokenHandler.WriteToken(newToken);

            // Rotate refresh token: revoke old, save new
            await userRepo.RevokeRefreshTokenAsync(oldToken);
            var newRefresh = GenerateRefreshToken();
            var refreshExpires = DateTime.UtcNow.AddDays(authConfig.RefreshTokenExpirationDays);
            await userRepo.SaveRefreshTokenAsync(user.Username, newRefresh, refreshExpires);

            logger.LogInformation("Refresh token rotated for user {Username}", user.Username);
            return Results.Ok(new { token = newTokenString, refreshToken = newRefresh, username = user.Username, role = user.Role });
        });

        app.MapPost(endpointConfig.Revoke, async (
            RevokeRequest request,
            IUserRepository userRepo,
            ILogger<Program> logger) =>
        {
            await userRepo.RevokeRefreshTokenAsync(request.RefreshToken);
            logger.LogInformation("Refresh token revoked");
            return Results.Ok();
        });

        app.MapPost(endpointConfig.Register, async (
            RegisterRequest request,
            IValidator<RegisterRequest> validator,
            IUserRepository userRepo,
            IOperationLogger operationLogger,
            ILogger<Program> logger) =>
        {
            // Validate request using FluentValidation
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                operationLogger.LogValidationFailure("Register", request, validationResult.Errors);
                logger.LogWarning("Registration attempt with invalid data");
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var (success, errorMessage) = await userRepo.RegisterAsync(request.Username, request.Email, request.Password);
            if (!success)
            {
                logger.LogWarning("Failed registration attempt for user: {Username}, Error: {Error}", request.Username, errorMessage);
                return Results.BadRequest(new RegisterResponse(false, errorMessage));
            }

            logger.LogInformation("Successful registration for user: {Username}", request.Username);
            return Results.Created(endpointConfig.Register, new RegisterResponse(true, "Registration successful"));
        });

    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

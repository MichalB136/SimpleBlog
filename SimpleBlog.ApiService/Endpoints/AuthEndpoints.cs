using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using SimpleBlog.ApiService;
using SimpleBlog.Common;
using SimpleBlog.Common.Interfaces;
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

            var maskedLoginUsername = MaskUserName(request.Username);
            var user = await userRepo.ValidateUserAsync(request.Username, request.Password);
            if (user == null)
            {
                logger.LogWarning("Failed login attempt for user: {Username}", maskedLoginUsername);
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

            logger.LogInformation("Successful login for user: {Username}, Token length: {TokenLength}", MaskUserName(user.Username), tokenString.Length);
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

            logger.LogInformation("Refresh token rotated for user {Username}", MaskUserName(user.Username));
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
            IEmailService emailService,
            IConfiguration configuration,
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

            var maskedRegisterUsername = MaskUserName(request.Username);
            var (success, errorMessage) = await userRepo.RegisterAsync(request.Username, request.Email, request.Password);
            if (!success)
            {
                logger.LogWarning("Failed registration attempt for user: {Username}, Error: {Error}", maskedRegisterUsername, errorMessage);
                return Results.BadRequest(new RegisterResponse(false, errorMessage));
            }

            var maskedEmail = MaskEmail(request.Email);

            // Send email confirmation
            try
            {
                var (token, tokenError) = await userRepo.GenerateEmailConfirmationTokenAsync(request.Email);
                
                if (token is not null && tokenError is null)
                {
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                    var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
                    var confirmationLink = $"{frontendBaseUrl}/confirm-email?token={encodedToken}";

                    await emailService.SendEmailConfirmationAsync(request.Email, confirmationLink);
                    logger.LogInformation("Email confirmation sent to {Email} for user {Username}", maskedEmail, maskedRegisterUsername);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email confirmation for {Email}", maskedEmail);
                // Continue with registration even if email sending fails
            }

            logger.LogInformation("Successful registration for user: {Username}", maskedRegisterUsername);
            return Results.Created(endpointConfig.Register, new RegisterResponse(true, "Registration successful. Please check your email to confirm your account."));
        });

        app.MapPost("/auth/request-password-reset", async (
            RequestPasswordResetRequest request,
            IUserRepository userRepo,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<Program> logger) =>
        {
            var maskedEmail = MaskEmail(request.Email);
            var (token, error) = await userRepo.GeneratePasswordResetTokenAsync(request.Email);
            
            if (error is not null)
            {
                logger.LogWarning("Error generating password reset token for {Email}: {Error}", maskedEmail, error);
                // Return neutral response to prevent email enumeration
                return Results.Ok(new OperationResponse(true, "If the email exists, a password reset link has been sent."));
            }

            if (token is null)
            {
                // Email doesn't exist - return neutral response
                logger.LogWarning("Password reset requested for non-existent email: {Email}", maskedEmail);
                return Results.Ok(new OperationResponse(true, "If the email exists, a password reset link has been sent."));
            }

            try
            {
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
                var resetLink = $"{frontendBaseUrl}/reset-password?token={encodedToken}";

                await emailService.SendPasswordResetEmailAsync(request.Email, resetLink);
                logger.LogInformation("Password reset email sent to {Email}", maskedEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send password reset email to {Email}", maskedEmail);
                // Still return success to prevent email enumeration
            }

            return Results.Ok(new OperationResponse(true, "If the email exists, a password reset link has been sent."));
        });

        app.MapPost("/auth/reset-password", async (
            PasswordResetRequest request,
            IUserRepository userRepo,
            ILogger<Program> logger) =>
        {
            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
                var (success, error) = await userRepo.ResetPasswordAsync(request.UserId, decodedToken, request.NewPassword);

                if (!success)
                {
                    logger.LogWarning("Failed password reset attempt for user {UserId}: {Error}", request.UserId, error);
                    return Results.BadRequest(new OperationResponse(false, error ?? "Failed to reset password"));
                }

                logger.LogInformation("Password successfully reset for user {UserId}", request.UserId);
                return Results.Ok(new OperationResponse(true, "Password has been reset successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error resetting password for user {UserId}", request.UserId);
                return Results.BadRequest(new OperationResponse(false, "Invalid or expired reset token"));
            }
        });

        app.MapPost("/auth/confirm-email", async (
            ConfirmEmailRequest request,
            IUserRepository userRepo,
            ILogger<Program> logger) =>
        {
            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
                var (success, error) = await userRepo.ConfirmEmailAsync(request.UserId, decodedToken);

                if (!success)
                {
                    logger.LogWarning("Failed email confirmation for user {UserId}: {Error}", request.UserId, error);
                    return Results.BadRequest(new OperationResponse(false, error ?? "Failed to confirm email"));
                }

                logger.LogInformation("Email successfully confirmed for user {UserId}", request.UserId);
                return Results.Ok(new OperationResponse(true, "Email has been confirmed successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error confirming email for user {UserId}", request.UserId);
                return Results.BadRequest(new OperationResponse(false, "Invalid or expired confirmation token"));
            }
        });

    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
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

    private static string MaskUserName(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "unknown";
        }

        return username.Length <= 2
            ? $"{username[0]}*"
            : $"{username[..1]}***";
    }
}

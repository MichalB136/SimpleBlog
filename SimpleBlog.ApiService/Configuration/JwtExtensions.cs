using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using static SimpleBlog.ApiService.SeedDataConstants;

namespace SimpleBlog.ApiService.Configuration;

/// <summary>
/// Extension methods for JWT authentication configuration
/// </summary>
public static class JwtExtensions
{
    public static (string Issuer, string Audience, byte[] Key) ConfigureJwt(this WebApplicationBuilder builder)
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtConfig["Key"] ?? throw new InvalidOperationException("JWT:Key is not configured");
        var jwtIssuer = jwtConfig["Issuer"] ?? "SimpleBlog";
        var jwtAudience = jwtConfig["Audience"] ?? "SimpleBlog";
        var key = Encoding.UTF8.GetBytes(jwtKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "JWT authentication failed.");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var subject = MaskSubject(context.Principal?.Identity?.Name);
                    var claimsCount = context.Principal?.Claims?.Count() ?? 0;
                    logger.LogInformation("JWT token validated. Subject: {Subject}, ClaimsCount: {ClaimsCount}", subject, claimsCount);
                    return Task.CompletedTask;
                }
            };
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole(AdminRole));

        return (jwtIssuer, jwtAudience, key);
    }

    private static string MaskSubject(string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            return "unknown";
        }

        return subject.Length <= 2
            ? $"{subject[0]}*"
            : $"{subject[..1]}***";
    }
}

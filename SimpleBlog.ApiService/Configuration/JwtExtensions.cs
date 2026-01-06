using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

        var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("JwtConfiguration");
        logger.LogInformation("JWT Config - Key length: {KeyLength}, Issuer: {Issuer}, Audience: {Audience}", jwtKey.Length, jwtIssuer, jwtAudience);

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
                    logger.LogError("JWT Authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
                    logger.LogInformation("JWT Token validated successfully. Claims: {Claims}", string.Join(", ", claims ?? Array.Empty<string>()));
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

        builder.Services.AddAuthorization();

        return (jwtIssuer, jwtAudience, key);
    }
}

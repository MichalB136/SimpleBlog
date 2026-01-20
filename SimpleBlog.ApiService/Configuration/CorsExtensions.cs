namespace SimpleBlog.ApiService.Configuration;

/// <summary>
/// Extension methods for CORS configuration
/// </summary>
public static class CorsExtensions
{
    private static readonly string[] DefaultAllowedOrigins =
    [
        "http://localhost:5080",
        "http://localhost:7166",
        "https://localhost:7166"
    ];

    public static void ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            // Configure CORS based on environment
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? DefaultAllowedOrigins;
            
            options.AddPolicy("AllowDevClients", policy =>
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
    }
}

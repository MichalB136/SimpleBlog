namespace SimpleBlog.ApiService.Services;

using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleBlog.Common.Interfaces;

public static class CloudinarySetup
{
    public sealed record Result(bool Configured, string? Message);

    public static Result Configure(IServiceCollection services, IConfiguration configuration)
    {
        var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
        Cloudinary? cloudinary = null;

        if (!string.IsNullOrEmpty(cloudinaryUrl))
        {
            // Use CLOUDINARY_URL format: cloudinary://api_key:api_secret@cloud_name
            cloudinary = new Cloudinary(cloudinaryUrl)
            {
                Api = { Secure = true }
            };
            services.AddSingleton(cloudinary);
            services.AddScoped<IImageStorageService, CloudinaryStorageService>();
            return new Result(true, "Cloudinary configured from CLOUDINARY_URL");
        }
        else
        {
            // Fallback to individual settings
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                cloudinary = new Cloudinary(account)
                {
                    Api = { Secure = true }
                };
                services.AddSingleton(cloudinary);
                services.AddScoped<IImageStorageService, CloudinaryStorageService>();
                return new Result(true, $"Cloudinary configured with CloudName: {cloudName}");
            }
            else
            {
                // Register a no-op image storage so endpoints that accept image operations
                // can still be invoked (they will be no-ops). This prevents the endpoint
                // discovery from inferring body parameters when the service is missing.
                services.AddScoped<IImageStorageService, NoOpImageStorageService>();

                // Build a presence map (do not expose secret values)
                var foundCloudinaryUrl = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
                var foundCloudName = !string.IsNullOrEmpty(cloudName);
                var foundApiKey = !string.IsNullOrEmpty(apiKey);
                var foundApiSecret = !string.IsNullOrEmpty(apiSecret);

                var details = $"Presence: CLOUDINARY_URL={foundCloudinaryUrl}, CloudName={foundCloudName}, ApiKey={foundApiKey}, ApiSecret={foundApiSecret}";

                return new Result(false,
                    "Cloudinary not configured. Image upload features will not be available. " +
                    "Set CLOUDINARY_URL environment variable (cloudinary://api_key:api_secret@cloud_name) " +
                    "or individual variables: SimpleBlog_Cloudinary__CloudName, SimpleBlog_Cloudinary__ApiKey, SimpleBlog_Cloudinary__ApiSecret. " +
                    details);
            }
        }
    }
}

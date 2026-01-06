using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Configuration;

/// <summary>
/// Extension methods for configuring logging services.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures operation logging services for the application.
    /// </summary>
    public static IServiceCollection ConfigureOperationLogging(this IServiceCollection services)
    {
        // Register operation logger
        services.AddScoped<IOperationLogger, OperationLogger>();

        // Register HttpContextAccessor for user context
        services.AddHttpContextAccessor();

        return services;
    }
}

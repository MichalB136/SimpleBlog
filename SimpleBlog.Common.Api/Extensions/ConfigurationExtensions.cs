namespace SimpleBlog.Common.Api.Extensions;

/// <summary>
/// Extension methods for configuring shared application settings from configuration hierarchy.
/// 
/// Configuration loading order (lowest to highest priority):
/// 1. appsettings.shared.json (base configuration)
/// 2. appsettings.shared.{Environment}.json (environment-specific overrides)
/// 3. Environment variables with "SimpleBlog_" prefix (highest priority)
/// </summary>
public static class ConfigurationExtensions
{
    private const string SharedConfigFileName = "appsettings.shared.json";
    private const string EnvironmentVariablePrefix = "SimpleBlog_";

    /// <summary>
    /// Adds shared configuration from appsettings hierarchy.
    /// 
    /// Configuration is loaded in this order:
    /// 1. appsettings.shared.json (required)
    /// 2. appsettings.shared.{environment}.json (optional, environment-specific)
    /// 3. Environment variables prefixed with "SimpleBlog_"
    /// </summary>
    /// <param name="configBuilder">The configuration builder.</param>
    /// <param name="environment">The environment name (Development, Staging, Production, etc.)</param>
    /// <param name="basePath">Optional base path where configuration files are located. Defaults to current directory.</param>
    /// <returns>The configuration builder for chaining.</returns>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Configuration.AddSharedConfiguration(builder.Environment.EnvironmentName);
    /// </code>
    /// </example>
    public static IConfigurationBuilder AddSharedConfiguration(
        this IConfigurationBuilder configBuilder,
        string environment,
        string? basePath = null)
    {
        // Default to solution root directory (parent of current directory)
        basePath ??= FindSolutionRoot(Directory.GetCurrentDirectory());

        // Load base configuration (required)
        configBuilder.AddJsonFile(
            Path.Combine(basePath, SharedConfigFileName),
            optional: false,
            reloadOnChange: true);

        // Load environment-specific configuration (optional)
        var environmentFileName = $"appsettings.shared.{environment}.json";
        configBuilder.AddJsonFile(
            Path.Combine(basePath, environmentFileName),
            optional: true,
            reloadOnChange: true);

        // Load environment variables (highest priority)
        configBuilder.AddEnvironmentVariables(EnvironmentVariablePrefix);

        return configBuilder;
    }

    /// <summary>
    /// Loads endpoint and authorization configurations from the configuration provider.
    /// </summary>
    /// <param name="configuration">The configuration source.</param>
    /// <returns>Tuple containing EndpointConfiguration and AuthorizationConfiguration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required configuration sections are missing.</exception>
    /// <example>
    /// <code>
    /// var config = builder.Configuration;
    /// var (endpoints, auth) = ConfigurationExtensions.LoadApiConfigurations(config);
    /// 
    /// app.Services.AddSingleton(endpoints);
    /// app.Services.AddSingleton(auth);
    /// </code>
    /// </example>
    public static (EndpointConfiguration Endpoints, AuthorizationConfiguration Authorization) LoadApiConfigurations(
        this IConfiguration configuration)
    {
        var endpointConfig = new EndpointConfiguration();
        var endpointSection = configuration.GetSection("Endpoints");
        if (!endpointSection.Exists())
            throw new InvalidOperationException("Required configuration section 'Endpoints' is missing from appsettings.shared.json");
        endpointSection.Bind(endpointConfig);

        var authConfig = new AuthorizationConfiguration();
        var authSection = configuration.GetSection("Authorization");
        if (!authSection.Exists())
            throw new InvalidOperationException("Required configuration section 'Authorization' is missing from appsettings.shared.json");
        authSection.Bind(authConfig);

        return (endpointConfig, authConfig);
    }

    /// <summary>
    /// Registers endpoint and authorization configurations in the dependency injection container.
    /// 
    /// This method loads configurations from the configuration hierarchy and registers them as singletons.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration source.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Configuration.AddSharedConfiguration(builder.Environment.EnvironmentName);
    /// builder.Services.AddApiConfigurations(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddApiConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var (endpoints, auth) = configuration.LoadApiConfigurations();

        services.AddSingleton(endpoints);
        services.AddSingleton(auth);

        return services;
    }

    /// <summary>
    /// Finds the solution root directory by looking for .sln file in parent directories.
    /// </summary>
    private static string FindSolutionRoot(string startPath)
    {
        var currentDir = new DirectoryInfo(startPath);
        
        while (currentDir != null)
        {
            // Check if .sln file exists in current directory
            if (currentDir.GetFiles("*.sln").Length > 0)
            {
                return currentDir.FullName;
            }
            
            // Move to parent directory
            currentDir = currentDir.Parent;
        }
        
        // Fallback to current directory if .sln not found
        return startPath;
    }
}

using FluentValidation;
using SimpleBlog.Common.Validators;

namespace SimpleBlog.ApiService.Configuration;

public static class ValidationExtensions
{
    /// <summary>
    /// Configures FluentValidation services and registers all validators from SimpleBlog.Common assembly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureValidation(this IServiceCollection services)
    {
        // Register all validators from the SimpleBlog.Common.Validators namespace
        services.AddValidatorsFromAssemblyContaining<CreatePostRequestValidator>();
        
        return services;
    }
}

using HRMarket.Middleware;

namespace HRMarket.Configuration.Exceptions;

/// <summary>
/// Extension methods for registering exception handling services
/// </summary>
public static class ExceptionHandlingServiceExtensions
{
    /// <summary>
    /// Registers all exception handling services including middleware, mappers, and handlers
    /// </summary>
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        // Register exception mapper
        services.AddSingleton<IExceptionMapper, ExceptionMapper>();

        // Register special exception handlers
        services.AddSingleton<ISpecialExceptionHandler, PostgresExceptionHandler>();
        
        // Add more special handlers here as needed:
        // services.AddSingleton<ISpecialExceptionHandler, StripeExceptionHandler>();
        // services.AddSingleton<ISpecialExceptionHandler, CustomExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Adds the exception handling middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
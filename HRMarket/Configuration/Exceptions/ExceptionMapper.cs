using System.Security;
using FluentValidation;

namespace HRMarket.Configuration.Exceptions;

/// <summary>
/// Maps exceptions to HTTP status codes and problem details
/// </summary>
public interface IExceptionMapper
{
    (int statusCode, ProblemDetails problemDetails) MapException(Exception exception, HttpContext context);
}

public class ExceptionMapper(ILogger<ExceptionMapper> logger) : IExceptionMapper
{
    public (int statusCode, ProblemDetails problemDetails) MapException(Exception exception, HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        return exception switch
        {
            ValidationException validationEx => MapValidationException(validationEx, context, traceId),
            NotFoundException notFoundEx => MapNotFoundException(notFoundEx, context, traceId),
            UnauthorizedAccessException unauthorizedEx => MapUnauthorizedException(unauthorizedEx, context, traceId),
            SecurityException securityEx => MapSecurityException(securityEx, context, traceId),
            BadHttpRequestException badRequestEx => MapBadRequestException(badRequestEx, context, traceId),
            ArgumentException argumentEx => MapArgumentException(argumentEx, context, traceId),
            _ => MapUnhandledException(exception, context, traceId)
        };
    }

    private (int, ProblemDetails) MapValidationException(
        ValidationException exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogWarning(exception, "Validation failed for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        // Group validation errors by property name (field name)
        var validationErrors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => ToCamelCase(g.Key), // Convert property name to camelCase
                g => g.Select(e => e.ErrorMessage).ToList() // All error messages for this field
            );

        var problemDetails = new ProblemDetails
        {
            Title = "One or more validation errors occurred",
            Detail = "Please check the validation errors and try again",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
            TraceId = traceId,
            ValidationErrors = validationErrors // This contains field -> errors mapping
        };

        return (StatusCodes.Status400BadRequest, problemDetails);
    }

    private (int, ProblemDetails) MapNotFoundException(
        NotFoundException exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogWarning(exception, "Resource not found for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "Resource not found",
            Detail = exception.Message,
            Status = StatusCodes.Status404NotFound,
            Instance = context.Request.Path,
            TraceId = traceId
        };

        return (StatusCodes.Status404NotFound, problemDetails);
    }

    private (int, ProblemDetails) MapUnauthorizedException(
        UnauthorizedAccessException exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogWarning(exception, "Unauthorized access attempt for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "Unauthorized",
            Detail = "Authentication is required to access this resource",
            Status = StatusCodes.Status401Unauthorized,
            Instance = context.Request.Path,
            TraceId = traceId
        };

        return (StatusCodes.Status401Unauthorized, problemDetails);
    }

    private (int, ProblemDetails) MapSecurityException(
        SecurityException exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogWarning(exception, "Access forbidden for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "Forbidden",
            Detail = "You don't have permission to access this resource",
            Status = StatusCodes.Status403Forbidden,
            Instance = context.Request.Path,
            TraceId = traceId
        };

        return (StatusCodes.Status403Forbidden, problemDetails);
    }

    private (int, ProblemDetails) MapBadRequestException(
        BadHttpRequestException exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogWarning(exception, "Bad request for {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "Bad request",
            Detail = exception.Message,
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
            TraceId = traceId,
            Errors = [exception.Message]
        };

        return (StatusCodes.Status400BadRequest, problemDetails);
    }

    private (int, ProblemDetails) MapArgumentException(
        ArgumentException exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogWarning(exception, "Invalid argument for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "Invalid argument",
            Detail = exception.Message,
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
            TraceId = traceId,
            Errors = new List<string> { exception.Message }
        };

        return (StatusCodes.Status400BadRequest, problemDetails);
    }

    private (int, ProblemDetails) MapUnhandledException(
        Exception exception, 
        HttpContext context, 
        string traceId)
    {
        logger.LogError(exception, "Unhandled exception occurred for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred while processing your request",
            Detail = "An unexpected error occurred. Please try again later",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
            TraceId = traceId
        };

        return (StatusCodes.Status500InternalServerError, problemDetails);
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
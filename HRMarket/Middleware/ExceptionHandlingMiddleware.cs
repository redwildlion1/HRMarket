using System.Text.Json;
using System.Text.Json.Serialization;
using HRMarket.Configuration.Exceptions;

namespace HRMarket.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and converts them to standardized problem details responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IExceptionMapper _exceptionMapper;
    private readonly IEnumerable<ISpecialExceptionHandler> _specialHandlers;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IExceptionMapper exceptionMapper,
        IEnumerable<ISpecialExceptionHandler> specialHandlers)
    {
        _next = next;
        _logger = logger;
        _exceptionMapper = exceptionMapper;
        _specialHandlers = specialHandlers.OrderBy(h => h.Order);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Prevent writing to response if it has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Cannot write exception response, response has already started for {Path}",
                context.Request.Path);
            return;
        }

        try
        {
            // Try special handlers first
            foreach (var handler in _specialHandlers)
            {
                if (!handler.CanHandle(exception)) continue;
                var (statusCode, problemDetails) = await handler.HandleAsync(exception, context);
                await WriteResponseAsync(context, statusCode, problemDetails);
                return;
            }

            // Fall back to standard exception mapping
            var (code, details) = _exceptionMapper.MapException(exception, context);
            await WriteResponseAsync(context, code, details);
        }
        catch (Exception handlerException)
        {
            // If we fail to handle the exception, log it and try to send a generic error
            _logger.LogError(handlerException,
                "Failed to handle exception in middleware for {Path}",
                context.Request.Path);

            await WriteGenericErrorAsync(context);
        }
    }

    public async Task WriteResponseAsync(HttpContext context, int statusCode, ProblemDetails problemDetails)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(problemDetails, _jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private async Task WriteGenericErrorAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            return;

        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred while processing your request",
            Status = StatusCodes.Status500InternalServerError,
            TraceId = context.TraceIdentifier
        };

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(problemDetails, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
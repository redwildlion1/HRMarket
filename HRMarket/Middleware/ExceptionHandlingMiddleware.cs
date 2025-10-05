using System.Security;
using System.Text.Json;
using FluentValidation;
using HRMarket.Configuration.Exceptions;
using Npgsql;

namespace HRMarket.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    ISpecialExceptionsHandler specialExceptionsHandler)
{
    private readonly Dictionary<Type, Func<Exception, HttpContext, Task>> _specialExceptionHandlersDictionary = new()
    {
        /*{ typeof(AddInvoiceException), specialExceptionsHandler.HandleAddInvoiceToDatabaseException },
        { typeof(StripeException), specialExceptionsHandler.HandleStripeException },*/
        { typeof(PostgresException), specialExceptionsHandler.HandlePostgresException }
    };

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (_specialExceptionHandlersDictionary.TryGetValue(ex.GetType(), out var specialHandler))
            {
                await specialHandler(ex, context);
                return;
            }

            await HandleNormalException(ex, context);
        }
    }

    private Task HandleNormalException(Exception ex, HttpContext context)
    {
        var statusCode = ex switch
        {
            BadHttpRequestException or ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            SecurityException => StatusCodes.Status403Forbidden,
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode >= 500)
            logger.LogError(ex, "Unhandled server exception occurred");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Title = ex.Message,
            Errors = ex is ValidationException validationException
                ? validationException.Errors.Select(e => e.ErrorMessage).ToList()
                : [],
            Status = statusCode
        };

        var json = JsonSerializer.Serialize(problem);
        return context.Response.WriteAsync(json);
    }

    public static Task WriteResponse(HttpContext context, int statusCode, string message, List<string>? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Title = message,
            Errors = errors ?? [],
            Status = statusCode
        };

        var json = JsonSerializer.Serialize(problem);
        return context.Response.WriteAsync(json);
    }
}

public class ProblemDetails
{
    public string Title { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];
    public int Status { get; set; }
}
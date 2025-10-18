namespace HRMarket.Configuration.Exceptions;

/// <summary>
/// Interface for handling special exceptions that require custom processing
/// </summary>
public interface ISpecialExceptionHandler
{
    /// <summary>
    /// Determines if this handler can handle the given exception
    /// </summary>
    bool CanHandle(Exception exception);

    /// <summary>
    /// Handles the exception and returns the status code and problem details
    /// </summary>
    Task<(int statusCode, ProblemDetails problemDetails)> HandleAsync(Exception exception, HttpContext context);

    /// <summary>
    /// The order in which this handler should be executed (lower values execute first)
    /// </summary>
    int Order { get; }
}
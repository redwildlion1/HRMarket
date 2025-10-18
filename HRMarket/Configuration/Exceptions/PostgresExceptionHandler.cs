using Npgsql;

namespace HRMarket.Configuration.Exceptions;

/// <summary>
/// Handles PostgreSQL-specific exceptions and converts them to user-friendly messages
/// </summary>
public class PostgresExceptionHandler(ILogger<PostgresExceptionHandler> logger) : ISpecialExceptionHandler
{
    public int Order => 100;

    public bool CanHandle(Exception exception)
    {
        return exception is PostgresException;
    }

    public Task<(int statusCode, ProblemDetails problemDetails)> HandleAsync(
        Exception exception, 
        HttpContext context)
    {
        var postgresException = (PostgresException)exception;
        var traceId = context.TraceIdentifier;

        logger.LogError(postgresException, 
            "PostgreSQL error occurred: Code={SqlState}, Message={Message}, TraceId={TraceId}", 
            postgresException.SqlState, 
            postgresException.MessageText,
            traceId);

        var (statusCode, title, detail, validationErrors) = MapPostgresError(postgresException);

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path,
            TraceId = traceId,
            ValidationErrors = validationErrors
        };

        return Task.FromResult((statusCode, problemDetails));
    }

    private (int statusCode, string title, string detail, Dictionary<string, List<string>>? validationErrors) 
        MapPostgresError(PostgresException exception)
    {
        // PostgreSQL error codes: https://www.postgresql.org/docs/current/errcodes-appendix.html
        return exception.SqlState switch
        {
            // Unique violation (23505)
            "23505" => HandleUniqueViolation(exception),
            
            // Foreign key violation (23503)
            "23503" => HandleForeignKeyViolation(exception),
            
            // Not null violation (23502)
            "23502" => HandleNotNullViolation(exception),
            
            // Check violation (23514)
            "23514" => HandleCheckViolation(exception),
            
            // String data right truncation (22001)
            "22001" => HandleDataTruncation(exception),
            
            // Numeric value out of range (22003)
            "22003" => HandleNumericOutOfRange(exception),
            
            // Invalid text representation (22P02)
            "22P02" => HandleInvalidTextRepresentation(exception),
            
            // Deadlock detected (40P01)
            "40P01" => (
                StatusCodes.Status409Conflict,
                "Conflict occurred",
                "A conflict occurred while processing your request. Please try again.",
                null
            ),
            
            // Connection errors (08xxx)
            { } code when code.StartsWith("08") => (
                StatusCodes.Status503ServiceUnavailable,
                "Service temporarily unavailable",
                "The database service is temporarily unavailable. Please try again later.",
                null
            ),
            
            // Default for unknown errors
            _ => (
                StatusCodes.Status500InternalServerError,
                "Database error occurred",
                "An unexpected database error occurred. Please try again later.",
                null
            )
        };
    }

    private (int, string, string, Dictionary<string, List<string>>?) HandleUniqueViolation(PostgresException exception)
    {
        var fieldName = ExtractConstraintField(exception.ConstraintName);
        var message = $"A record with this {fieldName} already exists";

        var validationErrors = new Dictionary<string, List<string>>();
        if (!string.IsNullOrEmpty(fieldName))
        {
            validationErrors[ToCamelCase(fieldName)] = new List<string> { message };
        }

        return (
            StatusCodes.Status409Conflict,
            "Duplicate entry",
            message,
            validationErrors.Count > 0 ? validationErrors : null
        );
    }

    private static (int, string, string, Dictionary<string, List<string>>?) HandleForeignKeyViolation(PostgresException exception)
    {
        var fieldName = ExtractConstraintField(exception.ConstraintName);
        var message = string.IsNullOrEmpty(fieldName)
            ? "The referenced record does not exist or cannot be deleted because it's referenced by other records"
            : $"Invalid {fieldName}: the referenced record does not exist";

        var validationErrors = new Dictionary<string, List<string>>();
        if (!string.IsNullOrEmpty(fieldName))
        {
            validationErrors[ToCamelCase(fieldName)] = new List<string> { message };
        }

        return (
            StatusCodes.Status400BadRequest,
            "Invalid reference",
            message,
            validationErrors.Count > 0 ? validationErrors : null
        );
    }

    private static (int, string, string, Dictionary<string, List<string>>?) HandleNotNullViolation(PostgresException exception)
    {
        var fieldName = exception.ColumnName ?? ExtractConstraintField(exception.ConstraintName);
        var message = string.IsNullOrEmpty(fieldName)
            ? "A required field is missing"
            : $"{FormatFieldName(fieldName)} is required";

        var validationErrors = new Dictionary<string, List<string>>();
        if (!string.IsNullOrEmpty(fieldName))
        {
            validationErrors[ToCamelCase(fieldName)] = new List<string> { message };
        }

        return (
            StatusCodes.Status400BadRequest,
            "Required field missing",
            message,
            validationErrors.Count > 0 ? validationErrors : null
        );
    }

    private static (int, string, string, Dictionary<string, List<string>>?) HandleCheckViolation(PostgresException exception)
    {
        var constraintName = exception.ConstraintName ?? string.Empty;
        const string message = "The provided value does not meet the required constraints";

        // Try to extract field name from constraint
        var fieldName = ExtractConstraintField(constraintName);
        
        var validationErrors = new Dictionary<string, List<string>>();
        if (!string.IsNullOrEmpty(fieldName))
        {
            validationErrors[ToCamelCase(fieldName)] = new List<string> { message };
        }

        return (
            StatusCodes.Status400BadRequest,
            "Constraint violation",
            message,
            validationErrors.Count > 0 ? validationErrors : null
        );
    }

    private static (int, string, string, Dictionary<string, List<string>>?) HandleDataTruncation(PostgresException exception)
    {
        var fieldName = exception.ColumnName ?? string.Empty;
        var message = string.IsNullOrEmpty(fieldName)
            ? "The provided value is too long"
            : $"{FormatFieldName(fieldName)} is too long";

        var validationErrors = new Dictionary<string, List<string>>();
        if (!string.IsNullOrEmpty(fieldName))
        {
            validationErrors[ToCamelCase(fieldName)] = [message];
        }

        return (
            StatusCodes.Status400BadRequest,
            "Value too long",
            message,
            validationErrors.Count > 0 ? validationErrors : null
        );
    }

    private (int, string, string, Dictionary<string, List<string>>?) HandleNumericOutOfRange(PostgresException exception)
    {
        var fieldName = exception.ColumnName ?? string.Empty;
        var message = string.IsNullOrEmpty(fieldName)
            ? "The provided numeric value is out of range"
            : $"{FormatFieldName(fieldName)} is out of the acceptable range";

        var validationErrors = new Dictionary<string, List<string>>();
        if (!string.IsNullOrEmpty(fieldName))
        {
            validationErrors[ToCamelCase(fieldName)] = [message];
        }

        return (
            StatusCodes.Status400BadRequest,
            "Value out of range",
            message,
            validationErrors.Count > 0 ? validationErrors : null
        );
    }

    private static (int, string, string, Dictionary<string, List<string>>?) HandleInvalidTextRepresentation(PostgresException exception)
    {
        const string message = "The provided value has an invalid format";

        return (
            StatusCodes.Status400BadRequest,
            "Invalid format",
            message,
            null
        );
    }

    private static string ExtractConstraintField(string? constraintName)
    {
        if (string.IsNullOrEmpty(constraintName))
            return string.Empty;

        // Common patterns: uk_users_email, fk_orders_userid, chk_price_positive
        var parts = constraintName.Split('_', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            // Skip constraint type prefix (uk, fk, pk, chk, etc.)
            > 2 => parts[^1],
            > 1 => parts[1],
            _ => string.Empty
        };
    }

    private static string FormatFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return fieldName;

        // Convert snake_case or PascalCase to readable format
        return string.Join(" ", fieldName.Split('_'))
            .Replace("  ", " ")
            .Trim();
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
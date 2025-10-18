namespace HRMarket.Configuration.Exceptions;

/// <summary>
/// Standardized error response format following RFC 7807 Problem Details specification
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// A URI reference that identifies the specific occurrence of the problem
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// The time when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier for tracing this error in logs
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Field-specific validation errors (field name -> list of error messages)
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }

    /// <summary>
    /// General errors not tied to specific fields
    /// </summary>
    public List<string>? Errors { get; set; }
}
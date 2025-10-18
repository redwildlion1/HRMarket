namespace HRMarket.Core;

/// <summary>
/// Base class for all DTOs that support localization
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Language code for validation errors (e.g., "ro", "en")
    /// </summary>
    public string Language { get; set; } = "ro";
}
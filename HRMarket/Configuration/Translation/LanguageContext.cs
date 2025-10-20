namespace HRMarket.Configuration.Translation;

/// <summary>
/// Scoped service that holds the current request's language
/// Automatically populated from Accept-Language header by middleware
/// </summary>
public interface ILanguageContext
{
    string Language { get; set; }
}

public class LanguageContext : ILanguageContext
{
    public string Language { get; set; } = "ro";
}
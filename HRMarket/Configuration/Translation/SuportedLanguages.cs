namespace HRMarket.Configuration.Translation;

/// <summary>
/// Defines all supported languages in the application
/// </summary>
public static class SupportedLanguages
{
    public const string English = "en";
    public const string Romanian = "ro";
    
    public const string PrimaryLanguage = English;
    
    private static readonly HashSet<string> SupportedLanguagesSet =
    [
        English,
        Romanian
    ];

    public static IReadOnlyCollection<string> All => SupportedLanguagesSet;

    public static bool IsSupported(string language)
    {
        return SupportedLanguagesSet.Contains(language?.ToLower() ?? string.Empty);
    }

    private static void ValidateLanguage(string language)
    {
        if (!IsSupported(language))
        {
            throw new ArgumentException(
                $"Language '{language}' is not supported. Supported languages: {string.Join(", ", All)}");
        }
    }
    
    /// <summary>
    /// Returns the list of languages an answer should be stored in based on the request language
    /// ONLY applies to String/Text type questions
    /// </summary>
    public static List<string> GetStorageLanguages(string requestLanguage)
    {
        ValidateLanguage(requestLanguage);

        // If requesting in English, store only in English
        return requestLanguage.Equals(English, StringComparison.OrdinalIgnoreCase) ? [English]
            :
            // If requesting in non-English, store in both English and that language
            [English, requestLanguage.ToLower()];
    }
    
    public static string GetDisplayName(string languageCode)
    {
        return languageCode.ToLower() switch
        {
            English => "English",
            Romanian => "Română",
            _ => languageCode
        };
    }
}
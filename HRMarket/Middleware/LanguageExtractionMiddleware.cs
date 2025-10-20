using HRMarket.Configuration.Translation;

namespace HRMarket.Middleware;

/// <summary>
/// Extracts language from Accept-Language header and sets it in LanguageContext
/// Runs early in the pipeline before validation or Identity operations
/// </summary>
public class LanguageExtractionMiddleware(
    RequestDelegate next,
    ILogger<LanguageExtractionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ILanguageContext languageContext)
    {
        var language = "ro"; // Default

        if (context.Request.Headers.TryGetValue("Accept-Language", out var languageHeader))
        {
            var headerValue = languageHeader.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                // Parse Accept-Language header (format: "en-US,en;q=0.9,ro;q=0.8")
                var primaryLanguage = headerValue.Split(',')
                    .FirstOrDefault()?
                    .Split(';')
                    .FirstOrDefault()?
                    .Trim();
                
                if (!string.IsNullOrWhiteSpace(primaryLanguage))
                {
                    // Extract language code (en-US -> en, ro-RO -> ro)
                    language = primaryLanguage.Split('-').FirstOrDefault()?.ToLower() ?? "ro";
                    
                    // Validate it's one of our supported languages
                    if (language != "en" && language != "ro")
                    {
                        language = "ro";
                    }
                }
            }
        }

        languageContext.Language = language;
        logger.LogDebug("Request language set to: {Language}", language);

        await next(context);
    }
}

public static class LanguageExtractionMiddlewareExtensions
{
    public static IApplicationBuilder UseLanguageExtraction(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LanguageExtractionMiddleware>();
    }
}
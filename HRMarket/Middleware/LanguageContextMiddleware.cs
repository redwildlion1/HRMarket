// HRMarket/Middleware/LanguageContextMiddleware.cs
using HRMarket.Configuration.Translation;

namespace HRMarket.Middleware;

public class LanguageContextMiddleware(
    RequestDelegate next,
    ILogger<LanguageContextMiddleware> logger)
{
    private const string AcceptLanguageHeader = "Accept-Language";
    private const string LanguageQueryParam = "lang";
    
    public async Task InvokeAsync(HttpContext context, LanguageContext languageContext)
    {
        var language = GetLanguageFromRequest(context);
        
        if (SupportedLanguages.IsSupported(language))
        {
            languageContext.Language = language;
        }
        else
        {
            languageContext.Language = SupportedLanguages.English;
            logger.LogWarning(
                "Unsupported language '{Language}' requested. Falling back to English.", 
                language);
        }
        
        await next(context);
    }
    
    private static string GetLanguageFromRequest(HttpContext context)
    {
        // Priority 1: Query parameter
        if (context.Request.Query.TryGetValue(LanguageQueryParam, out var queryLang))
        {
            return queryLang.ToString().ToLower();
        }
        
        // Priority 2: Accept-Language header
        if (!context.Request.Headers.TryGetValue(AcceptLanguageHeader, out var headerLang))
            return SupportedLanguages.English;
        var primaryLanguage = headerLang.ToString().Split(',').FirstOrDefault()?.Split(';').FirstOrDefault();
        return !string.IsNullOrEmpty(primaryLanguage) ? primaryLanguage.Trim().ToLower()[..Math.Min(2, primaryLanguage.Length)] :
            // Default
            SupportedLanguages.English;
    }
}
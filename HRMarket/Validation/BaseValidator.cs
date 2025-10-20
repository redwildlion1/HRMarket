using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core;

namespace HRMarket.Validation;

/// <summary>
/// Base validator that provides translation support from LanguageContext
/// </summary>
public abstract class BaseValidator<T>(
    ITranslationService translationService,
    ILanguageContext languageContext)
    : AbstractValidator<T>
    where T : BaseDto
{
    protected readonly ITranslationService TranslationService = translationService;
    protected readonly ILanguageContext LanguageContext = languageContext;

    /// <summary>
    /// Get translated error message using the current request language
    /// </summary>
    protected string Translate(string key, params object[] args)
    {
        return TranslationService.TranslateValidationError(key, LanguageContext.Language, args);
    }
}
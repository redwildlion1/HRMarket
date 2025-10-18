// HRMarket/Validation/BaseValidator.cs
using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core;

namespace HRMarket.Validation;

/// <summary>
/// Base validator that provides translation support based on DTO language
/// </summary>
public abstract class BaseValidator<T> : AbstractValidator<T> where T : BaseDto
{
    protected readonly ITranslationService TranslationService;

    protected BaseValidator(ITranslationService translationService)
    {
        TranslationService = translationService;
    }

    /// <summary>
    /// Get translated error message using the language from the DTO
    /// </summary>
    protected string Translate(string key, T instance, params object[] args)
    {
        var language = string.IsNullOrWhiteSpace(instance.Language) ? "ro" : instance.Language.ToLower();
        return TranslationService.TranslateValidationError(key, language, args);
    }

    /// <summary>
    /// Get translated error message using a specific language
    /// </summary>
    protected string Translate(string key, string language, params object[] args)
    {
        return TranslationService.TranslateValidationError(key, language, args);
    }
}
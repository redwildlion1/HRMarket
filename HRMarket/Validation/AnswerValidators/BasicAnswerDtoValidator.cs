// HRMarket/Validation/AnswerValidator.cs

using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Answers;

namespace HRMarket.Validation.AnswerValidators;

public class BasicAnswerDtoValidator : FluentValidation.AbstractValidator<BasicAnswerDto>
{
    private readonly ITranslationService _translationService;

    public BasicAnswerDtoValidator(ITranslationService translationService)
    {
        _translationService = translationService;

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage(x => GetMessage(ValidationErrorKeys.Required, x, "QuestionId"));

        RuleFor(x => x.Response)
            .NotEmpty()
            .WithMessage(x => GetMessage(ValidationErrorKeys.Required, x, "Response"));
    }

    private string GetMessage(string key, BasicAnswerDto instance, params object[] args)
    {
        var language = string.IsNullOrWhiteSpace(instance.Language) ? "ro" : instance.Language.ToLower();
        return _translationService.TranslateValidationError(key, language, args);
    }
}

public class SingleChoiceAnswerDtoValidator : FluentValidation.AbstractValidator<SingleChoiceAnswerDto>
{
    private readonly ITranslationService _translationService;

    public SingleChoiceAnswerDtoValidator(ITranslationService translationService)
    {
        _translationService = translationService;

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage(x => GetMessage(ValidationErrorKeys.Required, x, "QuestionId"));

        RuleFor(x => x.SelectedOption)
            .NotEmpty()
            .WithMessage(x => GetMessage(ValidationErrorKeys.Required, x, "SelectedOption"));
    }

    private string GetMessage(string key, SingleChoiceAnswerDto instance, params object[] args)
    {
        var language = string.IsNullOrWhiteSpace(instance.Language) ? "ro" : instance.Language.ToLower();
        return _translationService.TranslateValidationError(key, language, args);
    }
}

public class MultiChoiceAnswerDtoValidator : FluentValidation.AbstractValidator<MultiChoiceAnswerDto>
{
    private readonly ITranslationService _translationService;

    public MultiChoiceAnswerDtoValidator(ITranslationService translationService)
    {
        _translationService = translationService;

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage(x => GetMessage(ValidationErrorKeys.Required, x, "QuestionId"));

        RuleFor(x => x.SelectedOptions)
            .NotEmpty()
            .WithMessage(x => GetMessage(ValidationErrorKeys.Required, x, "SelectedOptions"))
            .Must(options => options.Count > 0)
            .WithMessage(x => GetMessage(ValidationErrorKeys.Question.OptionsMinCount, x, "Selected Options", 1));
    }

    private string GetMessage(string key, MultiChoiceAnswerDto instance, params object[] args)
    {
        var language = string.IsNullOrWhiteSpace(instance.Language) ? "ro" : instance.Language.ToLower();
        return _translationService.TranslateValidationError(key, language, args);
    }
}
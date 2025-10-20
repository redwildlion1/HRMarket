using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Answers;

namespace HRMarket.Validation.AnswerValidators;

public class BasicAnswerDtoValidator : AbstractValidator<BasicAnswerDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public BasicAnswerDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "QuestionId"));

        RuleFor(x => x.Response)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Response"));
    }

    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);
}

public class SingleChoiceAnswerDtoValidator : AbstractValidator<SingleChoiceAnswerDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public SingleChoiceAnswerDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "QuestionId"));

        RuleFor(x => x.SelectedOption)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "SelectedOption"));
    }

    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);
}

public class MultiChoiceAnswerDtoValidator : AbstractValidator<MultiChoiceAnswerDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public MultiChoiceAnswerDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "QuestionId"));

        RuleFor(x => x.SelectedOptions)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "SelectedOptions"))
            .Must(options => options.Count > 0)
            .WithMessage(T(ValidationErrorKeys.Question.OptionsMinCount, "Selected Options", 1));
    }

    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);
}
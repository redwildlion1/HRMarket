using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Questions.DTOs;
using Json.Schema;

namespace HRMarket.Validation.QuestionValidators;

public class CreateQuestionsForCategoryDtoValidator : BaseValidator<CreateQuestionsForCategoryDto>
{
    public CreateQuestionsForCategoryDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "CategoryId"));

        RuleFor(x => x.Questions)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Questions"))
            .Must(HaveUniqueOrders)
            .WithMessage(Translate(ValidationErrorKeys.Question.DuplicateOrder))
            .Must(HaveSequentialOrders)
            .WithMessage(Translate(ValidationErrorKeys.Question.OrderGaps));

        RuleForEach(x => x.Questions)
            .SetValidator(new PostQuestionDtoValidator(TranslationService, LanguageContext));
    }

    private static bool HaveUniqueOrders(List<PostQuestionDto> questions)
    {
        var orders = questions.Select(q => q.Order).ToList();
        return orders.Count == orders.Distinct().Count();
    }

    private static bool HaveSequentialOrders(List<PostQuestionDto> questions)
    {
        var orders = questions.Select(q => q.Order).OrderBy(o => o).ToList();
        return !orders.Where((t, i) => t != i).Any();
    }
}

public class PostQuestionDtoValidator : AbstractValidator<PostQuestionDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public PostQuestionDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Title"));

        RuleFor(x => x.Type)
            .Must(BeValidQuestionType)
            .WithMessage(T(ValidationErrorKeys.Question.TypeInvalid));

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage(T(ValidationErrorKeys.MustBePositive, "Order"));

        When(x => x.Type == nameof(QuestionType.SingleSelect), () =>
        {
            RuleFor(x => x.Options)
                .NotNull()
                .WithMessage(T(ValidationErrorKeys.Question.OptionsRequired, "SingleSelect"))
                .Must(options => options != null && options.Count >= 2)
                .WithMessage(T(ValidationErrorKeys.Question.OptionsMinCount, "SingleSelect", 2))
                .DependentRules(() =>
                {
                    RuleFor(x => x.Options)
                        .Must(HaveUniqueOrders!)
                        .WithMessage(T(ValidationErrorKeys.Option.DuplicateOrder))
                        .Must(HaveSequentialOrders!)
                        .WithMessage(T(ValidationErrorKeys.Option.OrderGaps));
                    
                    RuleForEach(x => x.Options)
                        .SetValidator(new PostOptionDtoValidator(_translationService, _languageContext));
                });
        });

        When(x => x.Type == nameof(QuestionType.MultiSelect), () =>
        {
            RuleFor(x => x.Options)
                .NotNull()
                .WithMessage(T(ValidationErrorKeys.Question.OptionsRequired, "MultiSelect"))
                .Must(options => options != null && options.Count >= 2)
                .WithMessage(T(ValidationErrorKeys.Question.OptionsMinCount, "MultiSelect", 2))
                .DependentRules(() =>
                {
                    RuleFor(x => x.Options)
                        .Must(HaveUniqueOrders!)
                        .WithMessage(T(ValidationErrorKeys.Option.DuplicateOrder))
                        .Must(HaveSequentialOrders!)
                        .WithMessage(T(ValidationErrorKeys.Option.OrderGaps));
                    
                    RuleForEach(x => x.Options)
                        .SetValidator(new PostOptionDtoValidator(_translationService, _languageContext));
                });
        });

        When(x => x.Type != nameof(QuestionType.SingleSelect) && x.Type != nameof(QuestionType.MultiSelect), () =>
        {
            RuleFor(x => x.Options)
                .Must(options => options == null || options.Count == 0)
                .WithMessage(x => T(ValidationErrorKeys.Question.OptionsShouldBeEmpty, x.Type));
        });

        When(x => IsBasicQuestionType(x.Type) && !string.IsNullOrWhiteSpace(x.ValidationJson), () =>
        {
            RuleFor(x => x.ValidationJson)
                .Must(BeValidJsonSchema)
                .WithMessage(T(ValidationErrorKeys.Question.InvalidJsonSchema));
        });

        When(x => x.Variants is { Count: > 0 }, () =>
        {
            RuleFor(x => x.Variants)
                .Must(HaveUniqueLanguages!)
                .WithMessage(T(ValidationErrorKeys.Question.VariantDuplicateLanguage, "{0}"));
            
            RuleForEach(x => x.Variants)
                .SetValidator(new PostQuestionVariantDtoValidator(_translationService, _languageContext));
        });
    }

    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);

    private static bool BeValidQuestionType(string type) => 
        Enum.TryParse<QuestionType>(type, true, out _);

    private static bool IsBasicQuestionType(string type)
    {
        if (!Enum.TryParse<QuestionType>(type, true, out var questionType))
            return false;
        return questionType is QuestionType.String or QuestionType.Text or QuestionType.Number or QuestionType.Date;
    }

    private static bool BeValidJsonSchema(string? validationJson)
    {
        if (string.IsNullOrWhiteSpace(validationJson) || validationJson == "null")
            return true;
        try
        {
            _ = JsonSchema.FromText(validationJson);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HaveUniqueOrders(List<PostOptionDto> options) => 
        options.Select(o => o.Order).Distinct().Count() == options.Count;

    private static bool HaveSequentialOrders(List<PostOptionDto> options)
    {
        var orders = options.Select(o => o.Order).OrderBy(o => o).ToList();
        return !orders.Where((t, i) => t != i).Any();
    }

    private static bool HaveUniqueLanguages(List<PostQuestionVariantDto> variants) => 
        variants.Select(v => v.LanguageId).Distinct().Count() == variants.Count;
}

public class PostOptionDtoValidator : AbstractValidator<PostOptionDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public PostOptionDtoValidator(ITranslationService translationService, ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Text"));

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage(T(ValidationErrorKeys.MustBePositive, "Order"));

        When(x => x.Variants != null && x.Variants.Count > 0, () =>
        {
            RuleFor(x => x.Variants)
                .Must(variants => variants!.Select(v => v.LanguageId).Distinct().Count() == variants!.Count)
                .WithMessage(T(ValidationErrorKeys.Question.VariantDuplicateLanguage, "{0}"));
            
            RuleForEach(x => x.Variants)
                .SetValidator(new PostOptionVariantDtoValidator(_translationService, _languageContext));
        });
    }

    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);
}

public class PostQuestionVariantDtoValidator : AbstractValidator<PostQuestionVariantDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public PostQuestionVariantDtoValidator(ITranslationService translationService, ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.LanguageId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "LanguageId"))
            .MaximumLength(10)
            .WithMessage(T(ValidationErrorKeys.MaxLength, "LanguageId", 10));

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Title"));
    }

    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);
}

public class PostOptionVariantDtoValidator : AbstractValidator<PostOptionVariantDto>
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageContext _languageContext;

    public PostOptionVariantDtoValidator(ITranslationService translationService, ILanguageContext languageContext)
    {
        _translationService = translationService;
        _languageContext = languageContext;

        RuleFor(x => x.LanguageId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "LanguageId"))
            .MaximumLength(10)
            .WithMessage(T(ValidationErrorKeys.MaxLength, "LanguageId", 10));

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Value"));
    }
    
    private string T(string key, params object[] args) => 
        _translationService.TranslateValidationError(key, _languageContext.Language, args);
}
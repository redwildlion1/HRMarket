// HRMarket/Validation/QuestionValidator.cs

using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Questions.DTOs;
using Json.Schema;

namespace HRMarket.Validation.QuestionValidators;

public class CreateQuestionsForCategoryDtoValidator : BaseValidator<CreateQuestionsForCategoryDto>
{
    public CreateQuestionsForCategoryDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "CategoryId"));

        RuleFor(x => x.Questions)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Questions"))
            .Must(HaveUniqueOrders)
            .WithMessage(x => Translate(ValidationErrorKeys.Question.DuplicateOrder, x))
            .Must(HaveSequentialOrders)
            .WithMessage(x => Translate(ValidationErrorKeys.Question.OrderGaps, x));

        RuleForEach(x => x.Questions)
            .SetValidator((dto, _) => new PostQuestionDtoValidator(TranslationService, dto.Language));
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

public class PostQuestionDtoValidator : FluentValidation.AbstractValidator<PostQuestionDto>
{
    private readonly ITranslationService _translationService;
    private readonly string _language;

    public PostQuestionDtoValidator(ITranslationService translationService, string language = "ro")
    {
        _translationService = translationService;
        _language = string.IsNullOrWhiteSpace(language) ? "ro" : language.ToLower();

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
                        .SetValidator(new PostOptionDtoValidator(_translationService, _language));
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
                        .SetValidator(new PostOptionDtoValidator(_translationService, _language));
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
                .Must(BeValidJsonSchema!)
                .WithMessage(T(ValidationErrorKeys.Question.InvalidJsonSchema));
        });

        When(x => x.Variants is { Count: > 0 }, () =>
        {
            RuleFor(x => x.Variants)
                .Must(HaveUniqueLanguages!)
                .WithMessage(T(ValidationErrorKeys.Question.VariantDuplicateLanguage, "{0}"));
            
            RuleForEach(x => x.Variants)
                .SetValidator(new PostQuestionVariantDtoValidator(_translationService, _language));
        });
    }

    private string T(string key, params object[] args)
    {
        return _translationService.TranslateValidationError(key, _language, args);
    }

    private static bool BeValidQuestionType(string type)
    {
        return Enum.TryParse<QuestionType>(type, true, out _);
    }

    private static bool IsBasicQuestionType(string type)
    {
        if (!Enum.TryParse<QuestionType>(type, true, out var questionType))
            return false;

        return questionType is QuestionType.String or QuestionType.Text 
            or QuestionType.Number or QuestionType.Date;
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

    private static bool HaveUniqueOrders(List<PostOptionDto> options)
    {
        var orders = options.Select(o => o.Order).ToList();
        return orders.Count == orders.Distinct().Count();
    }

    private static bool HaveSequentialOrders(List<PostOptionDto> options)
    {
        var orders = options.Select(o => o.Order).OrderBy(o => o).ToList();
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i] != i)
                return false;
        }
        return true;
    }

    private static bool HaveUniqueLanguages(List<PostQuestionVariantDto> variants)
    {
        var languages = variants.Select(v => v.LanguageId).ToList();
        return languages.Count == languages.Distinct().Count();
    }
}

public class PostOptionDtoValidator : FluentValidation.AbstractValidator<PostOptionDto>
{
    private readonly ITranslationService _translationService;
    private readonly string _language;

    public PostOptionDtoValidator(ITranslationService translationService, string language = "ro")
    {
        _translationService = translationService;
        _language = string.IsNullOrWhiteSpace(language) ? "ro" : language.ToLower();

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
                .SetValidator(new PostOptionVariantDtoValidator(_translationService, _language));
        });
    }

    private string T(string key, params object[] args)
    {
        return _translationService.TranslateValidationError(key, _language, args);
    }
}

public class PostQuestionVariantDtoValidator : FluentValidation.AbstractValidator<PostQuestionVariantDto>
{
    private readonly ITranslationService _translationService;
    private readonly string _language;

    public PostQuestionVariantDtoValidator(ITranslationService translationService, string language = "ro")
    {
        _translationService = translationService;
        _language = string.IsNullOrWhiteSpace(language) ? "ro" : language.ToLower();

        RuleFor(x => x.LanguageId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "LanguageId"))
            .MaximumLength(10)
            .WithMessage(T(ValidationErrorKeys.MaxLength, "LanguageId", 10));

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Title"));
    }

    private string T(string key, params object[] args)
    {
        return _translationService.TranslateValidationError(key, _language, args);
    }
}

public class PostOptionVariantDtoValidator : FluentValidation.AbstractValidator<PostOptionVariantDto>
{
    private readonly ITranslationService _translationService;
    private readonly string _language;

    public PostOptionVariantDtoValidator(ITranslationService translationService, string language = "ro")
    {
        _translationService = translationService;
        _language = string.IsNullOrWhiteSpace(language) ? "ro" : language.ToLower();

        RuleFor(x => x.LanguageId)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "LanguageId"))
            .MaximumLength(10)
            .WithMessage(T(ValidationErrorKeys.MaxLength, "LanguageId", 10));

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage(T(ValidationErrorKeys.Required, "Value"));
    }
    
    private string T(string key, params object[] args)
    {
        return _translationService.TranslateValidationError(key, _language, args);
    }
}
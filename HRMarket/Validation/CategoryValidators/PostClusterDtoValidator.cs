using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Categories.DTOs;

namespace HRMarket.Validation.CategoryValidators;

public class TranslationDtoValidator : AbstractValidator<TranslationDto>
{
    public TranslationDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext)
    {
        var language = languageContext.Language;

        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .WithMessage(translationService.Translate(ValidationErrorKeys.Required, language, "Language Code"))
            .Must(SupportedLanguages.IsSupported)
            .WithMessage(translationService.Translate(
                ValidationErrorKeys.Question.VariantLanguageInvalid, 
                language));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(translationService.Translate(ValidationErrorKeys.Required, language, "Name"))
            .MaximumLength(100)
            .WithMessage(translationService.Translate(ValidationErrorKeys.MaxLength, language, "Name", 100));

        RuleFor(x => x.Description)
            .MaximumLength(300)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage(translationService.Translate(ValidationErrorKeys.MaxLength, language, "Description", 300));
    }
}

public class PostClusterDtoValidator : BaseValidator<PostClusterDto>
{
    public PostClusterDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Icon"))
            .MaximumLength(255)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Icon", 255));

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Translations"))
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");

        RuleForEach(x => x.Translations)
            .SetValidator(new TranslationDtoValidator(translationService, languageContext));
    }

    private static bool HaveAllSupportedLanguages(List<TranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class PostCategoryDtoValidator : BaseValidator<PostCategoryDto>
{
    public PostCategoryDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Icon"))
            .MaximumLength(255)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Icon", 255));

        When(x => x.OrderInCluster.HasValue, () =>
        {
            RuleFor(x => x.OrderInCluster!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Order"));
        });

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Translations"))
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");

        RuleForEach(x => x.Translations)
            .SetValidator(new TranslationDtoValidator(translationService, languageContext));
    }

    private static bool HaveAllSupportedLanguages(List<TranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class PostServiceDtoValidator : BaseValidator<PostServiceDto>
{
    public PostServiceDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.OrderInCategory)
            .GreaterThanOrEqualTo(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Order"));

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "CategoryId"));

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Translations"))
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");

        RuleForEach(x => x.Translations)
            .SetValidator(new TranslationDtoValidator(translationService, languageContext));
    }

    private static bool HaveAllSupportedLanguages(List<TranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class UpdateClusterDtoValidator : BaseValidator<UpdateClusterDto>
{
    public UpdateClusterDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Id"));

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Icon"))
            .MaximumLength(255)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Icon", 255));

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Translations"))
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");

        RuleForEach(x => x.Translations)
            .SetValidator(new TranslationDtoValidator(translationService, languageContext));
    }

    private static bool HaveAllSupportedLanguages(List<TranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class UpdateCategoryDtoValidator : BaseValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Id"));

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Icon"))
            .MaximumLength(255)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Icon", 255));

        When(x => x.OrderInCluster.HasValue, () =>
        {
            RuleFor(x => x.OrderInCluster!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Order"));
        });

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Translations"))
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");

        RuleForEach(x => x.Translations)
            .SetValidator(new TranslationDtoValidator(translationService, languageContext));
    }

    private static bool HaveAllSupportedLanguages(List<TranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class UpdateServiceDtoValidator : BaseValidator<UpdateServiceDto>
{
    public UpdateServiceDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Id"));

        RuleFor(x => x.OrderInCategory)
            .GreaterThanOrEqualTo(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Order"));

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "CategoryId"));

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Translations"))
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");

        RuleForEach(x => x.Translations)
            .SetValidator(new TranslationDtoValidator(translationService, languageContext));
    }

    private static bool HaveAllSupportedLanguages(List<TranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class AddCategoryToClusterDtoValidator : BaseValidator<AddCategoryToClusterDto>
{
    public AddCategoryToClusterDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "CategoryId"));

        RuleFor(x => x.ClusterId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "ClusterId"));
    }
}
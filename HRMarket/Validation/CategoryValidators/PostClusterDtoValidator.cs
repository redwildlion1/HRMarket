using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Categories.DTOs;

namespace HRMarket.Validation.CategoryValidators;

public class PostClusterDtoValidator : BaseValidator<PostClusterDto>
{
    public PostClusterDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Name"))
            .MaximumLength(100)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Name", 100));

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Icon"))
            .MaximumLength(255)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Icon", 255));
    }
}

public class PostCategoryDtoValidator : BaseValidator<PostCategoryDto>
{
    public PostCategoryDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Name"))
            .MaximumLength(100)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Name", 100));

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
    }
}

public class PostServiceDtoValidator : BaseValidator<PostServiceDto>
{
    public PostServiceDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Name"))
            .MaximumLength(100)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Name", 100));

        RuleFor(x => x.OrderInCategory)
            .GreaterThanOrEqualTo(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Order"));

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "CategoryId"));
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
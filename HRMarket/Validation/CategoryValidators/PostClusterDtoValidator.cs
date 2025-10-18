// HRMarket/Validation/CategoryValidator.cs

using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Categories.DTOs;

namespace HRMarket.Validation.CategoryValidators;

public class PostClusterDtoValidator : BaseValidator<PostClusterDto>
{
    public PostClusterDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Name"))
            .MaximumLength(100)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Name", 100));

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Icon"))
            .MaximumLength(255)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Icon", 255));
    }
}

public class PostCategoryDtoValidator : BaseValidator<PostCategoryDto>
{
    public PostCategoryDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Name"))
            .MaximumLength(100)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Name", 100));

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Icon"))
            .MaximumLength(255)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Icon", 255));

        When(x => x.OrderInCluster.HasValue, () =>
        {
            RuleFor(x => x.OrderInCluster!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage(x => Translate(ValidationErrorKeys.MustBePositive, x, "Order"));
        });
    }
}

public class PostServiceDtoValidator : BaseValidator<PostServiceDto>
{
    public PostServiceDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Name"))
            .MaximumLength(100)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Name", 100));

        RuleFor(x => x.OrderInCategory)
            .GreaterThanOrEqualTo(0)
            .WithMessage(x => Translate(ValidationErrorKeys.MustBePositive, x, "Order"));

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "CategoryId"));
    }
}

public class AddCategoryToClusterDtoValidator : BaseValidator<AddCategoryToClusterDto>
{
    public AddCategoryToClusterDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "CategoryId"));

        RuleFor(x => x.ClusterId)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "ClusterId"));
    }
}
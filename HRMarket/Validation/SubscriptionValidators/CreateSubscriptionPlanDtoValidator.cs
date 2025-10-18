// HRMarket/Validation/StripeValidator.cs

using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.StripeApi;

namespace HRMarket.Validation.SubscriptionValidators;

public class CreateSubscriptionPlanDtoValidator : BaseValidator<CreateSubscriptionPlanDto>
{
    public CreateSubscriptionPlanDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Name"))
            .MaximumLength(50)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Name", 50));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Description"))
            .MaximumLength(500)
            .WithMessage(x => Translate(ValidationErrorKeys.MaxLength, x, "Description", 500));

        RuleFor(x => x.PriceMonthly)
            .GreaterThan(0)
            .WithMessage(x => Translate(ValidationErrorKeys.MustBePositive, x, "Monthly Price"));

        RuleFor(x => x.PriceYearly)
            .GreaterThan(0)
            .WithMessage(x => Translate(ValidationErrorKeys.MustBePositive, x, "Yearly Price"));

        RuleFor(x => x.Features)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Features"))
            .Must(features => features.Count > 0)
            .WithMessage(x => Translate(ValidationErrorKeys.Question.OptionsMinCount, x, "Features", 1));
    }
}

public class CreateCheckoutSessionDtoValidator : BaseValidator<CreateCheckoutSessionDto>
{
    public CreateCheckoutSessionDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.FirmId)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "FirmId"));

        RuleFor(x => x.PriceId)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "PriceId"));
    }
}
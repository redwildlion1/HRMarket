using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.StripeApi;

namespace HRMarket.Validation.SubscriptionValidators;

public class CreateSubscriptionPlanDtoValidator : BaseValidator<CreateSubscriptionPlanDto>
{
    public CreateSubscriptionPlanDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Name"))
            .MaximumLength(50)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Name", 50));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Description"))
            .MaximumLength(500)
            .WithMessage(Translate(ValidationErrorKeys.MaxLength, "Description", 500));

        RuleFor(x => x.PriceMonthly)
            .GreaterThan(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Monthly Price"));

        RuleFor(x => x.PriceYearly)
            .GreaterThan(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Yearly Price"));

        RuleFor(x => x.Features)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Features"))
            .Must(features => features.Count > 0)
            .WithMessage(Translate(ValidationErrorKeys.Question.OptionsMinCount, "Features", 1));
    }
}

public class CreateCheckoutSessionDtoValidator : BaseValidator<CreateCheckoutSessionDto>
{
    public CreateCheckoutSessionDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.FirmId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "FirmId"));

        RuleFor(x => x.PriceId)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "PriceId"));
    }
}
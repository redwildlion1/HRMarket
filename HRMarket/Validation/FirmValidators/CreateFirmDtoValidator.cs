using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Firms.DTOs;

namespace HRMarket.Validation.FirmValidators;

public class CreateFirmDtoValidator : BaseValidator<CreateFirmDto>
{
    public CreateFirmDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Cui)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "CUI"))
            .Length(12)
            .WithMessage(Translate(ValidationErrorKeys.Firm.CuiInvalid))
            .Must(BeAllDigits)
            .WithMessage(Translate(ValidationErrorKeys.Firm.CuiFormat));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Name"));

        RuleFor(x => x.Type)
            .Must(type => Enum.TryParse<FirmType>(type, true, out _))
            .WithMessage(Translate(ValidationErrorKeys.Firm.TypeInvalid));

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Email"))
            .EmailAddress()
            .WithMessage(Translate(ValidationErrorKeys.EmailInvalid));
        
        When(x => !string.IsNullOrEmpty(x.LinksWebsite), () =>
        {
            RuleFor(x => x.LinksWebsite)
                .Must(BeValidUrl!)
                .WithMessage(Translate(ValidationErrorKeys.InvalidUrl));
        });
        
        When(x => !string.IsNullOrEmpty(x.LinksLinkedIn), () =>
        {
            RuleFor(x => x.LinksLinkedIn)
                .Must(uri => BeValidUrl(uri!) && uri!.Contains("linkedin.com"))
                .WithMessage(Translate(ValidationErrorKeys.SocialMedia.LinkedInInvalid));
        });

        When(x => !string.IsNullOrEmpty(x.LinksFacebook), () =>
        {
            RuleFor(x => x.LinksFacebook)
                .Must(uri => BeValidUrl(uri!) && uri!.Contains("facebook.com"))
                .WithMessage(Translate(ValidationErrorKeys.SocialMedia.FacebookInvalid));
        });
        
        When(x => !string.IsNullOrEmpty(x.LinksTwitter), () =>
        {
            RuleFor(x => x.LinksTwitter)
                .Must(uri => BeValidUrl(uri!) && (uri!.Contains("twitter.com") || uri.Contains("x.com")))
                .WithMessage(Translate(ValidationErrorKeys.SocialMedia.TwitterInvalid));
        });
        
        When(x => !string.IsNullOrEmpty(x.LinksInstagram), () =>
        {
            RuleFor(x => x.LinksInstagram)
                .Must(uri => BeValidUrl(uri!) && uri!.Contains("instagram.com"))
                .WithMessage(Translate(ValidationErrorKeys.SocialMedia.InstagramInvalid));
        });

        RuleFor(x => x.LocationCity)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "City"));

        RuleFor(x => x.LocationCountryId)
            .GreaterThan(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "Country"));

        RuleFor(x => x.LocationCountyId)
            .GreaterThan(0)
            .WithMessage(Translate(ValidationErrorKeys.MustBePositive, "County"));
    }

    private static bool BeValidUrl(string uri) => Uri.IsWellFormedUriString(uri, UriKind.Absolute);
    private static bool BeAllDigits(string cui) => cui.All(char.IsDigit);
}
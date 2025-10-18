// HRMarket/Validation/FirmValidator.cs

using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Firms.DTOs;

namespace HRMarket.Validation.FirmValidators;

public class CreateFirmDtoValidator : BaseValidator<CreateFirmDto>
{
    public CreateFirmDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.Cui)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "CUI"))
            .Length(12)
            .WithMessage(x => Translate(ValidationErrorKeys.Firm.CuiInvalid, x))
            .Must(BeAllDigits)
            .WithMessage(x => Translate(ValidationErrorKeys.Firm.CuiFormat, x));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Name"));

        RuleFor(x => x.Type)
            .Must(type => Enum.TryParse<FirmType>(type, true, out _))
            .WithMessage(x => Translate(ValidationErrorKeys.Firm.TypeInvalid, x));

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Email"))
            .EmailAddress()
            .WithMessage(x => Translate(ValidationErrorKeys.EmailInvalid, x));
        
        When(x => !string.IsNullOrEmpty(x.LinksWebsite), () =>
        {
            RuleFor(x => x.LinksWebsite)
                .Must(BeValidUrl!)
                .WithMessage(x => Translate(ValidationErrorKeys.InvalidUrl, x));
        });
        
        When(x => !string.IsNullOrEmpty(x.LinksLinkedIn), () =>
        {
            RuleFor(x => x.LinksLinkedIn)
                .Must(uri => BeValidUrl(uri!) && uri!.Contains("linkedin.com"))
                .WithMessage(x => Translate(ValidationErrorKeys.SocialMedia.LinkedInInvalid, x));
        });

        When(x => !string.IsNullOrEmpty(x.LinksFacebook), () =>
        {
            RuleFor(x => x.LinksFacebook)
                .Must(uri => BeValidUrl(uri!) && uri!.Contains("facebook.com"))
                .WithMessage(x => Translate(ValidationErrorKeys.SocialMedia.FacebookInvalid, x));
        });
        
        When(x => !string.IsNullOrEmpty(x.LinksTwitter), () =>
        {
            RuleFor(x => x.LinksTwitter)
                .Must(uri => BeValidUrl(uri!) && (uri!.Contains("twitter.com") || uri.Contains("x.com")))
                .WithMessage(x => Translate(ValidationErrorKeys.SocialMedia.TwitterInvalid, x));
        });
        
        When(x => !string.IsNullOrEmpty(x.LinksInstagram), () =>
        {
            RuleFor(x => x.LinksInstagram)
                .Must(uri => BeValidUrl(uri!) && uri!.Contains("instagram.com"))
                .WithMessage(x => Translate(ValidationErrorKeys.SocialMedia.InstagramInvalid, x));
        });

        RuleFor(x => x.LocationCity)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "City"));

        RuleFor(x => x.LocationCountryId)
            .GreaterThan(0)
            .WithMessage(x => Translate(ValidationErrorKeys.MustBePositive, x, "Country"));

        RuleFor(x => x.LocationCountyId)
            .GreaterThan(0)
            .WithMessage(x => Translate(ValidationErrorKeys.MustBePositive, x, "County"));
    }

    private static bool BeValidUrl(string uri)
    {
        return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
    }

    private static bool BeAllDigits(string cui)
    {
        return cui.All(char.IsDigit);
    }
}
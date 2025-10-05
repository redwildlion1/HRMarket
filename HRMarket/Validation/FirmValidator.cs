using FluentValidation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Firms.DTOs;

namespace HRMarket.Validation;

public class CreateFirmDTOValidator : AbstractValidator<CreateFirmDTO>
{
    public CreateFirmDTOValidator()
    {
        RuleFor(x => x.Type)
            .Must(type => Enum.IsDefined(typeof(FirmType), type))
            .WithMessage("Tipul firmei nu este valid.");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Vă rugăm să introduceți o adresă de email validă.");
        
        RuleFor(x => x.LinksWebsite)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Vă rugăm să introduceți un URL valid pentru site-ul web.");
        
        RuleFor(x => x.LinksLinkedIn) 
            .Must(uri => string.IsNullOrEmpty(uri) || (Uri.IsWellFormedUriString(uri, UriKind.Absolute) && uri.Contains("linkedin.com")))
            .WithMessage("Vă rugăm să introduceți un URL valid de LinkedIn (linkedin.com).");

        RuleFor(x => x.LinksFacebook)
            .Must(uri => string.IsNullOrEmpty(uri) || (Uri.IsWellFormedUriString(uri, UriKind.Absolute) && uri.Contains("facebook.com")))
            .WithMessage("Vă rugăm să introduceți un URL valid de Facebook (facebook.com).");
        
        RuleFor(x => x.LinksTwitter)
            .Must(uri => string.IsNullOrEmpty(uri) || (Uri.IsWellFormedUriString(uri, UriKind.Absolute) && uri.Contains("twitter.com")))
            .WithMessage("Vă rugăm să introduceți un URL valid de Twitter (twitter.com).");
        
        RuleFor(x => x.LinksInstagram)
            .Must(uri => string.IsNullOrEmpty(uri) || (Uri.IsWellFormedUriString(uri, UriKind.Absolute) && uri.Contains("instagram.com")))
            .WithMessage("Vă rugăm să introduceți un URL valid de Instagram (instagram.com).");
        
        
        
    }
}
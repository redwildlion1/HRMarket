using FluentValidation;
using HRMarket.Configuration;
using HRMarket.Configuration.Translation;
using HRMarket.Core;
using HRMarket.Core.Auth;

namespace HRMarket.Validation.AuthValidators;

public class RegisterDtoValidator : BaseValidator<RegisterDto>
{
    public RegisterDtoValidator(
        ITranslationService translationService,
        ILanguageContext languageContext) 
        : base(translationService, languageContext)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Email"))
            .EmailAddress()
            .WithMessage(Translate(ValidationErrorKeys.EmailInvalid));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(Translate(ValidationErrorKeys.Required, "Password"))
            .MinimumLength(AppConstants.PasswordMinLength)
            .WithMessage(Translate(ValidationErrorKeys.Auth.PasswordTooShort, AppConstants.PasswordMinLength));
    }
}
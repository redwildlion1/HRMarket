// HRMarket/Validation/AuthValidator.cs

using FluentValidation;
using HRMarket.Configuration;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Auth;

namespace HRMarket.Validation.AuthValidators;

public class RegisterDtoValidator : BaseValidator<RegisterDto>
{
    public RegisterDtoValidator(ITranslationService translationService) 
        : base(translationService)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Email"))
            .EmailAddress()
            .WithMessage(x => Translate(ValidationErrorKeys.EmailInvalid, x));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(x => Translate(ValidationErrorKeys.Required, x, "Password"))
            .MinimumLength(AppConstants.PasswordMinLength)
            .WithMessage(x => Translate(ValidationErrorKeys.Auth.PasswordTooShort, x, AppConstants.PasswordMinLength));
    }
}
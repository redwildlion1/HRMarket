using FluentValidation;
using HRMarket.Configuration;
using HRMarket.Core.Auth;

namespace HRMarket.Validation;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress()
            .WithMessage("Vă rugăm să introduceți o adresă de email validă.");
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(AppConstants.PasswordMinLength)
            .WithMessage($"Parola trebuie să aibă cel puțin {AppConstants.PasswordMinLength} caractere.");
    }
}
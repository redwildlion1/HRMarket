using FluentValidation;
using HRMarket.Configuration;

namespace HRMarket.Core.Auth;

public class RegisterValidator : AbstractValidator<RegisterDTO>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(AppConstants.PasswordMinLength);
    }
}
using HRMarket.Configuration.Translation;
using Microsoft.AspNetCore.Identity;

namespace HRMarket.Core.Auth.Extensions;

/// <summary>
/// Custom Identity error describer that uses translation service
/// Gets language from scoped LanguageContext
/// </summary>
public class CustomIdentityErrorDescriber(
    ITranslationService translationService,
    ILanguageContext languageContext)
    : IdentityErrorDescriber
{
    private string T(string key, params object[] args) => 
        translationService.Translate(key, languageContext.Language, args);

    public override IdentityError DefaultError() => 
        new() { Code = nameof(DefaultError), Description = T(ValidationErrorKeys.Auth.DefaultError) };

    public override IdentityError ConcurrencyFailure() => 
        new() { Code = nameof(ConcurrencyFailure), Description = T(ValidationErrorKeys.Auth.ConcurrencyFailure) };

    public override IdentityError PasswordMismatch() => 
        new() { Code = nameof(PasswordMismatch), Description = T(ValidationErrorKeys.Auth.PasswordMismatch) };

    public override IdentityError InvalidToken() => 
        new() { Code = nameof(InvalidToken), Description = T(ValidationErrorKeys.Auth.InvalidToken) };

    public override IdentityError LoginAlreadyAssociated() => 
        new() { Code = nameof(LoginAlreadyAssociated), Description = T(ValidationErrorKeys.Auth.LoginAlreadyAssociated) };

    public override IdentityError InvalidUserName(string? userName) => 
        new() { Code = nameof(InvalidUserName), Description = T(ValidationErrorKeys.Auth.InvalidUserName) };

    public override IdentityError InvalidEmail(string? email) => 
        new() { Code = nameof(InvalidEmail), Description = T(ValidationErrorKeys.Auth.InvalidEmail) };

    public override IdentityError DuplicateUserName(string userName) => 
        new() { Code = nameof(DuplicateUserName), Description = T(ValidationErrorKeys.Auth.DuplicateUserName) };

    public override IdentityError DuplicateEmail(string email) => 
        new() { Code = nameof(DuplicateEmail), Description = T(ValidationErrorKeys.Auth.DuplicateEmail) };

    public override IdentityError InvalidRoleName(string? role) => 
        new() { Code = nameof(InvalidRoleName), Description = T(ValidationErrorKeys.Auth.InvalidRoleName) };

    public override IdentityError DuplicateRoleName(string role) => 
        new() { Code = nameof(DuplicateRoleName), Description = T(ValidationErrorKeys.Auth.DuplicateRoleName) };

    public override IdentityError UserAlreadyHasPassword() => 
        new() { Code = nameof(UserAlreadyHasPassword), Description = T(ValidationErrorKeys.Auth.UserAlreadyHasPassword) };

    public override IdentityError UserLockoutNotEnabled() => 
        new() { Code = nameof(UserLockoutNotEnabled), Description = T(ValidationErrorKeys.Auth.UserLockoutNotEnabled) };

    public override IdentityError UserAlreadyInRole(string role) => 
        new() { Code = nameof(UserAlreadyInRole), Description = T(ValidationErrorKeys.Auth.UserAlreadyInRole) };

    public override IdentityError UserNotInRole(string role) => 
        new() { Code = nameof(UserNotInRole), Description = T(ValidationErrorKeys.Auth.UserNotInRole) };

    public override IdentityError PasswordTooShort(int length) => 
        new() { Code = nameof(PasswordTooShort), Description = T(ValidationErrorKeys.Auth.PasswordTooShort, length) };

    public override IdentityError PasswordRequiresNonAlphanumeric() => 
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = T(ValidationErrorKeys.Auth.PasswordRequiresNonAlphanumeric) };

    public override IdentityError PasswordRequiresDigit() => 
        new() { Code = nameof(PasswordRequiresDigit), Description = T(ValidationErrorKeys.Auth.PasswordRequiresDigit) };

    public override IdentityError PasswordRequiresLower() => 
        new() { Code = nameof(PasswordRequiresLower), Description = T(ValidationErrorKeys.Auth.PasswordRequiresLowercase) };

    public override IdentityError PasswordRequiresUpper() => 
        new() { Code = nameof(PasswordRequiresUpper), Description = T(ValidationErrorKeys.Auth.PasswordRequiresUppercase) };

    public override IdentityError RecoveryCodeRedemptionFailed() => 
        new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = T(ValidationErrorKeys.Auth.RecoveryCodeRedemptionFailed) };
}
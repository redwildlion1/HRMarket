using FluentValidation;
using FluentValidation.Results;
using HRMarket.Configuration.Translation;
using Microsoft.AspNetCore.Identity;

namespace HRMarket.Core.Auth.Extensions;

/// <summary>
/// Clean extension methods for authentication operations
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Throws FluentValidation exception if IdentityResult failed
    /// </summary>
    public static void ThrowIfFailed(this IdentityResult result)
    {
        if (result.Succeeded) return;

        var failures = result.Errors
            .Select(e => new ValidationFailure(MapErrorCodeToField(e.Code), e.Description))
            .ToList();

        throw new ValidationException(failures);
    }

    /// <summary>
    /// Throws translated validation exception with custom message
    /// </summary>
    public static void ThrowAuthError(
        this ITranslationService translationService,
        string field,
        string errorKey,
        string language,
        params object[] args)
    {
        var message = translationService.Translate(errorKey, language, args);
        throw new ValidationException([new ValidationFailure(field, message)]);
    }

    /// <summary>
    /// Maps Identity error codes to appropriate field names for validation errors
    /// </summary>
    private static string MapErrorCodeToField(string errorCode) => errorCode switch
    {
        "DuplicateEmail" or "InvalidEmail" => "email",
        "DuplicateUserName" or "InvalidUserName" => "userName",
        "PasswordTooShort" or "PasswordRequiresNonAlphanumeric" or 
        "PasswordRequiresDigit" or "PasswordRequiresLower" or 
        "PasswordRequiresUpper" or "PasswordMismatch" or 
        "UserAlreadyHasPassword" => "password",
        "InvalidToken" => "token",
        "InvalidRoleName" or "DuplicateRoleName" or 
        "UserAlreadyInRole" or "UserNotInRole" => "role",
        "RecoveryCodeRedemptionFailed" => "recoveryCode",
        "LoginAlreadyAssociated" => "login",
        _ => string.Empty
    };
}
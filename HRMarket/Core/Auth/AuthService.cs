using HRMarket.Configuration.Translation;
using HRMarket.Core.Auth.Extensions;
using HRMarket.Core.Auth.Tokens;
using HRMarket.Entities.Auth;
using HRMarket.OuterAPIs.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HRMarket.Core.Auth;

public interface IAuthService
{
    Task Register(RegisterDto dto);
    Task<LoginResult> Login(LoginRequest request);
    Task<LoginResult> RefreshToken(RefreshTokenRequest request);
    Task ConfirmEmail(ConfirmEmailRequest request);
}

public class AuthService(
    UserManager<User> userManager,
    IEmailService emailService,
    ITokenService tokenService,
    IConfiguration configuration,
    ITranslationService translationService,
    ILanguageContext languageContext)
    : IAuthService
{
    public async Task Register(RegisterDto dto)
    {
        var user = new User
        {
            Email = dto.Email,
            UserName = dto.Email,
            Newsletter = dto.Newsletter,
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        result.ThrowIfFailed();

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        roleResult.ThrowIfFailed();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{configuration["AppSettings:FrontendUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await emailService.SendConfirmationEmail(user.Email, confirmationLink);
    }

    public async Task<LoginResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            translationService.ThrowAuthError(languageContext, "email", ValidationErrorKeys.Auth.InvalidCredentials);
        }

        if (!user!.EmailConfirmed)
        {
            translationService.ThrowAuthError(languageContext, "email", ValidationErrorKeys.Auth.EmailNotConfirmed);
        }

        var jwtToken = await tokenService.GenerateJwtTokenAsync(user);
        var refreshToken = await tokenService.GenerateRefreshToken(user.Id);

        return new LoginResult(jwtToken.Value, refreshToken.Value, jwtToken.Expires, refreshToken.Expires);
    }

    public async Task<LoginResult> RefreshToken(RefreshTokenRequest request)
    {
        var tokenHandler = new JsonWebTokenHandler();
        var jwtToken = tokenHandler.ReadJsonWebToken(request.Token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

        if (emailClaim == null)
        {
            translationService.ThrowAuthError(languageContext, "token", ValidationErrorKeys.Auth.InvalidToken);
        }

        var user = await userManager.FindByEmailAsync(emailClaim!.Value);
        if (user == null)
        {
            translationService.ThrowAuthError(languageContext, "token", ValidationErrorKeys.Auth.UserNotFound);
        }

        if (!await tokenService.IsRefreshTokenValidAsync(user!.Id, request.RefreshToken))
        {
            translationService.ThrowAuthError(languageContext, "refreshToken", ValidationErrorKeys.Auth.InvalidRefreshToken);
        }

        var newJwtToken = await tokenService.GenerateJwtTokenAsync(user);
        var newRefreshToken = await tokenService.GenerateRefreshToken(user.Id);

        await tokenService.InvalidateRefreshTokenAsync(user.Id, request.RefreshToken);

        return new LoginResult(newJwtToken.Value, newRefreshToken.Value, newJwtToken.Expires, newRefreshToken.Expires);
    }

    public async Task ConfirmEmail(ConfirmEmailRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            translationService.ThrowAuthError(languageContext, "userId", ValidationErrorKeys.Auth.UserNotFound);
        }

        var result = await userManager.ConfirmEmailAsync(user!, request.Token);
        result.ThrowIfFailed();
    }
}
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
    IAuthLanguageContext languageContext)
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

        // Create user - errors automatically translated via CustomIdentityErrorDescriber
        var result = await userManager.CreateAsync(user, dto.Password);
        result.ThrowIfFailed();

        // Assign default role
        var roleResult = await userManager.AddToRoleAsync(user, "User");
        roleResult.ThrowIfFailed();

        // Send confirmation email
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{configuration["AppSettings:FrontendUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await emailService.SendConfirmationEmail(user.Email!, confirmationLink);
    }

    public async Task<LoginResult> Login(LoginRequest request)
    {
        var lang = languageContext.Language;

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            translationService.ThrowAuthError("email", ValidationErrorKeys.Auth.InvalidCredentials, lang);
        }

        if (!user!.EmailConfirmed)
        {
            translationService.ThrowAuthError("email", ValidationErrorKeys.Auth.EmailNotConfirmed, lang);
        }

        var jwtToken = await tokenService.GenerateJwtTokenAsync(user);
        var refreshToken = await tokenService.GenerateRefreshToken(user.Id);

        return new LoginResult(jwtToken.Value, refreshToken.Value, jwtToken.Expires, refreshToken.Expires);
    }

    public async Task<LoginResult> RefreshToken(RefreshTokenRequest request)
    {
        var lang = languageContext.Language;

        var tokenHandler = new JsonWebTokenHandler();
        var jwtToken = tokenHandler.ReadJsonWebToken(request.Token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

        if (emailClaim == null)
        {
            translationService.ThrowAuthError("token", ValidationErrorKeys.Auth.InvalidToken, lang);
        }

        var user = await userManager.FindByEmailAsync(emailClaim!.Value);
        if (user == null)
        {
            translationService.ThrowAuthError("token", ValidationErrorKeys.Auth.UserNotFound, lang);
        }

        if (!await tokenService.IsRefreshTokenValidAsync(user!.Id, request.RefreshToken))
        {
            translationService.ThrowAuthError("refreshToken", ValidationErrorKeys.Auth.InvalidRefreshToken, lang);
        }

        var newJwtToken = await tokenService.GenerateJwtTokenAsync(user);
        var newRefreshToken = await tokenService.GenerateRefreshToken(user.Id);

        await tokenService.InvalidateRefreshTokenAsync(user.Id, request.RefreshToken);

        return new LoginResult(newJwtToken.Value, newRefreshToken.Value, newJwtToken.Expires, newRefreshToken.Expires);
    }

    public Task ConfirmEmail(ConfirmEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task ConfirmEmail(Guid userId, string token)
    {
        var lang = languageContext.Language;

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            translationService.ThrowAuthError("userId", ValidationErrorKeys.Auth.UserNotFound, lang);
        }

        var result = await userManager.ConfirmEmailAsync(user!, token);
        result.ThrowIfFailed();
    }
}
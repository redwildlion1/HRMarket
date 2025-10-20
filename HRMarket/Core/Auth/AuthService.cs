using HRMarket.Configuration.Redis;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Auth.Extensions;
using HRMarket.Core.Auth.Tokens;
using HRMarket.Entities.Auth;
using HRMarket.OuterAPIs.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HRMarket.Core.Auth;

public interface IAuthService
{
    Task Register(RegisterDto dto);
    Task<LoginResult> Login(LoginRequest request);
    Task<LoginResult> RefreshToken(RefreshTokenRequest request);
    Task ConfirmEmail(ConfirmEmailRequest request);
    Task<UserInfoDto> GetUserInfo(Guid userId);
    Task<bool> IsUserAdmin(Guid userId);
}

public class AuthService(
    UserManager<User> userManager,
    IEmailService emailService,
    ITokenService tokenService,
    IConfiguration configuration,
    ITranslationService translationService,
    ILanguageContext languageContext,
    IRedisService redis,
    Entities.ApplicationDbContext context)
    : IAuthService
{
    private static readonly TimeSpan UserInfoCacheDuration = TimeSpan.FromMinutes(5);

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

        // Get user info including firm details
        var userInfo = await GetUserInfo(user.Id);

        // Get the firm if it exists for token generation
        Entities.Firms.Firm? firm = null;
        if (userInfo.HasFirm && userInfo.FirmId.HasValue)
        {
            firm = await context.Firms.FindAsync(userInfo.FirmId.Value);
        }

        var jwtToken = await tokenService.GenerateJwtTokenAsync(user, firm);
        var refreshToken = await tokenService.GenerateRefreshToken(user.Id);

        return new LoginResult(
            jwtToken.Value, 
            refreshToken.Value, 
            jwtToken.Expires, 
            refreshToken.Expires,
            userInfo);
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

        // Get updated user info
        var userInfo = await GetUserInfo(user.Id);

        // Get the firm if it exists
        Entities.Firms.Firm? firm = null;
        if (userInfo.HasFirm && userInfo.FirmId.HasValue)
        {
            firm = await context.Firms.FindAsync(userInfo.FirmId.Value);
        }

        var newJwtToken = await tokenService.GenerateJwtTokenAsync(user, firm);
        var newRefreshToken = await tokenService.GenerateRefreshToken(user.Id);

        await tokenService.InvalidateRefreshTokenAsync(user.Id, request.RefreshToken);

        return new LoginResult(
            newJwtToken.Value, 
            newRefreshToken.Value, 
            newJwtToken.Expires, 
            newRefreshToken.Expires,
            userInfo);
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
        
        // Invalidate user info cache after email confirmation
        await redis.DeleteAsync(CacheKeys.Users.Info(request.UserId));
    }

    public async Task<UserInfoDto> GetUserInfo(Guid userId)
    {
        // Try to get from cache first
        var cacheKey = CacheKeys.Users.Info(userId);
        var cachedInfo = await redis.GetAsync<UserInfoDto>(cacheKey);
        if (cachedInfo != null)
        {
            return cachedInfo;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            translationService.ThrowAuthError(languageContext, "userId", ValidationErrorKeys.Auth.UserNotFound);
        }

        var roles = await userManager.GetRolesAsync(user!);
        var isAdmin = roles.Contains("Admin");

        // Check if user has a firm
        var firm = await context.Firms
            .Where(f => f.Contact != null && f.Contact.Email == user!.Email)
            .FirstOrDefaultAsync();

        var userInfo = new UserInfoDto
        {
            UserId = user!.Id,
            Email = user.Email!,
            IsAdmin = isAdmin,
            HasFirm = firm != null,
            FirmId = firm?.Id,
            FirmName = firm?.Name,
            Roles = roles.ToList()
        };

        // Cache the result
        await redis.SetAsync(cacheKey, userInfo, UserInfoCacheDuration);

        return userInfo;
    }

    public async Task<bool> IsUserAdmin(Guid userId)
    {
        // Try to get from cache first
        var cacheKey = CacheKeys.Users.Roles(userId);
        var cachedRoles = await redis.GetAsync<List<string>>(cacheKey);
        if (cachedRoles != null)
        {
            return cachedRoles.Contains("Admin");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var roles = await userManager.GetRolesAsync(user);
        
        // Cache roles
        await redis.SetAsync(cacheKey, roles.ToList(), UserInfoCacheDuration);
        
        return roles.Contains("Admin");
    }
}
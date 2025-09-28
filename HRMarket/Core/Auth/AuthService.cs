using HRMarket.Core.Auth.Tokens;
using HRMarket.Entities.Auth;
using HRMarket.OuterAPIs.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HRMarket.Core.Auth;

public interface IAuthService
{
    Task Register(RegisterDTO dto);
    Task<LoginResult> Login(LoginRequest request);
    Task<LoginResult> RefreshToken(RefreshTokenRequest request);
    Task ConfirmEmail(Guid userId, string token);
}

public class AuthService(UserManager<User> userManager, IEmailService emailService,ITokenService tokenService, IConfiguration configuration)
    : IAuthService
{
    public async Task Register(RegisterDTO dto)
    {
        var user = new User
        {
            Email = dto.Email,
            UserName = dto.Email,
            Newsletter = dto.Newsletter,
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{configuration["AppSettings:FrontendUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        
        await emailService.SendConfirmationEmail(user.Email, confirmationLink);
    }

    public async Task<LoginResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new Exception("Invalid email or password.");
        }

        var result = await userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            throw new Exception("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            throw new Exception("Email not confirmed. Please check your inbox.");
        }

        // Generate JWT and Refresh Token 
        var jwtToken = tokenService.GenerateJwtToken(user);
        // Also save the refresh token in the database or a persistent store
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
            throw new Exception("Invalid token.");
        }

        var user = await userManager.FindByEmailAsync(emailClaim.Value);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Validate the refresh token
        var isValidRefreshToken = await tokenService.IsRefreshTokenValidAsync(user.Id, request.RefreshToken);
        if (!isValidRefreshToken)
        {
            throw new Exception("Invalid refresh token.");
        }

        // Generate new JWT and Refresh Token
        var newJwtToken = tokenService.GenerateJwtToken(user);
        var newRefreshToken = await tokenService.GenerateRefreshToken(user.Id);

        // Invalidate the old refresh token
        await tokenService.InvalidateRefreshTokenAsync(user.Id, request.RefreshToken);
        return new LoginResult(newJwtToken.Value, newRefreshToken.Value, newJwtToken.Expires, newRefreshToken.Expires);
    }
    
    public async Task ConfirmEmail(Guid userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Email confirmation failed: {errors}");
        }
    }
}
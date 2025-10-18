using System.Security.Claims;
using HRMarket.Entities.Auth;
using HRMarket.Entities.Firms;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HRMarket.Core.Auth.Tokens;

public interface ITokenService
{
    Task<Token> GenerateJwtTokenAsync(User user, Firm? firm = null);
    Task<Token> GenerateRefreshToken(Guid userId);
    Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken);
    Task InvalidateRefreshTokenAsync(Guid userId, string refreshToken);
}

public class TokenService(
    ITokenRepository tokenRepository, 
    TokenSettings settings,
    UserManager<User> userManager) : ITokenService
{
    public async Task<Token> GenerateJwtTokenAsync(User user, Firm? firm = null)
    {
        var key = new SymmetricSecurityKey(settings.SecretKey.Select(c => (byte)c).ToArray());
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        // Add roles to claims
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        if (firm != null)
        {
            claims.Add(new Claim("FirmId", firm.Id.ToString()));
            claims.Add(new Claim("FirmName", firm.Name));
        }
    
        var expires = DateTime.UtcNow.AddMinutes(settings.JwtExpiryMinutes);
    
        var token = new JsonWebTokenHandler().CreateToken(
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = credentials,
                Issuer = settings.Issuer,
                Audience = settings.Audience
            });
        
        var tokenValue = token ?? throw new Exception("Failed to generate JWT token.");
    
        return new Token(tokenValue, expires);
    }

    public async Task<Token> GenerateRefreshToken(Guid userId)
    {
        var refreshTokenValue = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var expires = DateTime.UtcNow.AddDays(settings.RefreshTokenExpiryDays);
        await tokenRepository.SaveRefreshTokenAsync(userId, refreshTokenValue, expires);
        return new Token(refreshTokenValue, expires);
    }

    public Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken)
    {
        return tokenRepository.IsRefreshTokenValidAsync(userId, refreshToken);
    }

    public Task InvalidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        return tokenRepository.InvalidateRefreshTokenAsync(userId, refreshToken);
    }
}

public record Token(string Value, DateTime Expires);
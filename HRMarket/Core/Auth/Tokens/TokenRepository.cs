using HRMarket.Entities;
using HRMarket.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Auth.Tokens;

public interface ITokenRepository
{
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires);
    Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken);
    Task InvalidateRefreshTokenAsync(Guid userId, string refreshToken);
}

public class TokenRepository(ApplicationDbContext context) : ITokenRepository
{
    public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires)
    {
        var entity = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            Expires = expires,
        };
        context.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken)
    {
        var token = await context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken && t.IsActive);
        return token != null;
    }

    public async Task InvalidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var token = await context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken && t.IsActive);
        if (token != null)
        {
            token.Revoked = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}

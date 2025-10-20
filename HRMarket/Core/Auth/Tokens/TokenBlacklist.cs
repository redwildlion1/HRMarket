using HRMarket.Configuration.Redis;

namespace HRMarket.Core.Auth.Tokens;

public interface ITokenBlacklist
{
    Task BlacklistTokenAsync(string token, DateTime expiresAt);
    Task<bool> IsTokenBlacklistedAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task<bool> IsUserTokensRevokedAsync(Guid userId, DateTime tokenIssuedAt);
}

public class TokenBlacklist(
    IRedisService redis,
    ILogger<TokenBlacklist> logger) : ITokenBlacklist
{
    public async Task BlacklistTokenAsync(string token, DateTime expiresAt)
    {
        try
        {
            var key = CacheKeys.Tokens.Blacklist(token);
            var timeToLive = expiresAt - DateTime.UtcNow;
            
            if (timeToLive.TotalSeconds > 0)
            {
                await redis.SetStringAsync(key, "1", timeToLive);
                logger.LogInformation("Token blacklisted until {ExpiresAt}", expiresAt);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to blacklist token");
            throw;
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        try
        {
            var key = CacheKeys.Tokens.Blacklist(token);
            return await redis.ExistsAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if token is blacklisted");
            // Fail secure: if we can't check, treat as blacklisted
            return true;
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        try
        {
            var key = CacheKeys.Tokens.UserRevocation(userId);
            var revocationTime = DateTime.UtcNow.ToString("O");
            
            // Store revocation timestamp for 7 days (max refresh token lifetime)
            await redis.SetStringAsync(key, revocationTime, TimeSpan.FromDays(7));
            
            logger.LogWarning("All tokens revoked for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to revoke all user tokens for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsUserTokensRevokedAsync(Guid userId, DateTime tokenIssuedAt)
    {
        try
        {
            var key = CacheKeys.Tokens.UserRevocation(userId);
            var revocationTimeStr = await redis.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(revocationTimeStr))
                return false;
            
            if (!DateTime.TryParse(revocationTimeStr, out var revocationTime))
                return false;
            
            // Token is revoked if it was issued before the revocation time
            return tokenIssuedAt < revocationTime;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check user token revocation for user: {UserId}", userId);
            // Fail secure
            return true;
        }
    }
}
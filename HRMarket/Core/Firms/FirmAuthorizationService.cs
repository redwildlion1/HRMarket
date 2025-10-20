using HRMarket.Configuration.Redis;
using HRMarket.Core.Auth;
using HRMarket.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Firms;

public interface IFirmAuthorizationService
{
    Task<CheckFirmAccessResponse> CanUserEditFirm(Guid userId, Guid firmId);
    Task<bool> IsFirmOwner(Guid userId, Guid firmId);
    Task<Guid?> GetUserFirmId(Guid userId);
}

public class FirmAuthorizationService(
    ApplicationDbContext context,
    UserManager<Entities.Auth.User> userManager,
    IRedisService redis,
    ILogger<FirmAuthorizationService> logger) : IFirmAuthorizationService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public async Task<CheckFirmAccessResponse> CanUserEditFirm(Guid userId, Guid firmId)
    {
        // Check cache first
        var cacheKey = $"{CacheKeys.Firms.Owner(firmId)}:{userId}";
        var cachedResult = await redis.GetAsync<CheckFirmAccessResponse>(cacheKey);
        if (cachedResult != null)
        {
            logger.LogDebug("Firm access check cache hit for user {UserId} and firm {FirmId}", userId, firmId);
            return cachedResult;
        }

        // Check if user is admin
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new CheckFirmAccessResponse(false, false);
        }

        var roles = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains("Admin");

        // Check if user owns the firm
        var isFirmOwner = await IsFirmOwner(userId, firmId);

        // User can edit if they're admin OR they own the firm
        var canEdit = isAdmin || isFirmOwner;

        var response = new CheckFirmAccessResponse(canEdit, isFirmOwner);

        // Cache the result
        await redis.SetAsync(cacheKey, response, CacheDuration);

        return response;
    }

    public async Task<bool> IsFirmOwner(Guid userId, Guid firmId)
    {
        // Check cache first
        var cacheKey = CacheKeys.Firms.Owner(firmId);
        var cachedOwnerId = await redis.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedOwnerId) && Guid.TryParse(cachedOwnerId, out var cachedUserId))
        {
            return cachedUserId == userId;
        }

        // Get user's email
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return false;
        }

        // Check if firm exists and belongs to this user
        var firm = await context.Firms
            .Include(f => f.Contact)
            .FirstOrDefaultAsync(f => f.Id == firmId);

        if (firm?.Contact == null)
        {
            return false;
        }

        // Check if the firm's contact email matches the user's email
        var isOwner = firm.Contact.Email?.Equals(user.Email, StringComparison.OrdinalIgnoreCase) == true;

        // Cache the owner ID if found
        if (isOwner)
        {
            await redis.SetStringAsync(cacheKey, userId.ToString(), CacheDuration);
        }

        return isOwner;
    }

    public async Task<Guid?> GetUserFirmId(Guid userId)
    {
        // Check cache first
        var cacheKey = CacheKeys.Firms.UserFirms(userId);
        var cachedFirmId = await redis.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedFirmId) && Guid.TryParse(cachedFirmId, out var firmId))
        {
            return firmId;
        }

        // Get user's email
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return null;
        }

        // Find firm by contact email
        var firm = await context.Firms
            .Include(f => f.Contact)
            .FirstOrDefaultAsync(f => f.Contact != null && 
                                     f.Contact.Email == user.Email);

        if (firm == null)
        {
            return null;
        }

        // Cache the result
        await redis.SetStringAsync(cacheKey, firm.Id.ToString(), CacheDuration);

        return firm.Id;
    }
}
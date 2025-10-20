using HRMarket.Core.Auth.Tokens;
using HRMarket.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HRMarket.Middleware;

/// <summary>
/// Enhanced JWT validation with blacklist checking and optional DB validation for critical endpoints
/// </summary>
public class EnhancedJwtValidationMiddleware(
    RequestDelegate next,
    ILogger<EnhancedJwtValidationMiddleware> logger)
{
    // Paths that require strict DB validation on every request
    private static readonly HashSet<string> StrictValidationPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/me",
        "/api/auth/revoke-all-tokens",
        "/api/firms/create",
        "/api/subscriptions/checkout",
        "/api/admin"
    };

    public async Task InvokeAsync(
        HttpContext context,
        ITokenBlacklist tokenBlacklist,
        UserManager<User> userManager)
    {
        // Skip if no authorization header or if path doesn't require authentication
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !context.User.Identity?.IsAuthenticated == true)
        {
            await next(context);
            return;
        }

        var token = authHeader.ToString().Replace("Bearer ", "").Trim();
        if (string.IsNullOrEmpty(token))
        {
            await next(context);
            return;
        }

        // Level 1: Check token blacklist (fast cache check)
        if (await tokenBlacklist.IsTokenBlacklistedAsync(token))
        {
            logger.LogWarning("Blacklisted token attempted: {TokenPrefix}...", token[..Math.Min(10, token.Length)]);
            await WriteUnauthorizedResponse(context, "Token has been revoked");
            return;
        }

        // Level 2: Check if all user tokens were revoked
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            // Get token issue time from "iat" claim
            var iatClaim = context.User.FindFirst("iat")?.Value;
            if (iatClaim != null && long.TryParse(iatClaim, out var iat))
            {
                var tokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).UtcDateTime;
                if (await tokenBlacklist.IsUserTokensRevokedAsync(userId, tokenIssuedAt))
                {
                    logger.LogWarning("User {UserId} attempted to use revoked token", userId);
                    await WriteUnauthorizedResponse(context, "All tokens have been revoked. Please login again.");
                    return;
                }
            }
        }

        // Level 3: Strict DB validation for critical endpoints
        if (RequiresStrictValidation(context.Request.Path))
        {
            var isValid = await ValidateTokenAgainstDatabase(context, userManager, logger);
            if (!isValid)
            {
                await WriteUnauthorizedResponse(context, "Invalid credentials or insufficient permissions");
                return;
            }
        }

        await next(context);
    }

    private static bool RequiresStrictValidation(PathString path)
    {
        return StrictValidationPaths.Any(p => 
            path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<bool> ValidateTokenAgainstDatabase(
        HttpContext context,
        UserManager<User> userManager,
        ILogger logger)
    {
        try
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                logger.LogWarning("Invalid user ID in token claims");
                return false;
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                logger.LogWarning("Token valid but user not found: {UserId}", userId);
                return false;
            }

            // Check if email is still confirmed
            if (!user.EmailConfirmed)
            {
                logger.LogWarning("Token valid but email not confirmed: {UserId}", userId);
                return false;
            }

            // Check if user is locked out
            if (await userManager.IsLockedOutAsync(user))
            {
                logger.LogWarning("Token valid but user is locked out: {UserId}", userId);
                return false;
            }

            // Validate roles still match token claims
            var tokenRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var actualRoles = await userManager.GetRolesAsync(user);
            
            if (!tokenRoles.OrderBy(r => r).SequenceEqual(actualRoles.OrderBy(r => r)))
            {
                logger.LogWarning("Token roles don't match DB roles for user: {UserId}. Token: [{TokenRoles}], DB: [{DbRoles}]",
                    userId, 
                    string.Join(", ", tokenRoles),
                    string.Join(", ", actualRoles));
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating token against database");
            return false;
        }
    }

    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}

public static class EnhancedJwtValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseEnhancedJwtValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<EnhancedJwtValidationMiddleware>();
    }
}
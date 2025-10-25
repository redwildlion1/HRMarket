using HRMarket.Core.Auth.Examples;
using HRMarket.Core.Auth.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;

namespace HRMarket.Core.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ITokenBlacklist tokenBlacklist,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [SwaggerRequestExample(typeof(RegisterDto), typeof(RegisterExample))]
    public async Task<IActionResult> Register([FromBody]RegisterDto dto)
    {
        await authService.Register(dto);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginRequest request)
    {
        var result = await authService.Login(request);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshToken(request);
        return Ok(result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        var confirmEmailRequest = new ConfirmEmailRequest(userId, token);
        await authService.ConfirmEmail(confirmEmailRequest);
        return Ok();
    }

    /// <summary>
    /// Logout and immediately revoke the current token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get token from header
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "No token provided" });
            }
            
            // Get token expiration from claims
            var expClaim = User.FindFirst("exp")?.Value;
            if (expClaim == null || !long.TryParse(expClaim, out var exp))
                return Ok(new { message = "Logged out successfully" });
            
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            await tokenBlacklist.BlacklistTokenAsync(token, expiresAt);
                
            logger.LogInformation("User logged out successfully");

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Revoke all tokens for the current user (use for security incidents, password change, etc.)
    /// </summary>
    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            await tokenBlacklist.RevokeAllUserTokensAsync(userId);
            
            logger.LogWarning("All tokens revoked for user: {UserId}", userId);
            
            return Ok(new { message = "All tokens revoked successfully. Please login again." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking all tokens");
            return StatusCode(500, new { message = "An error occurred while revoking tokens" });
        }
    }

    /// <summary>
    /// Check if the current authenticated user has admin privileges
    /// Used for frontend routing to /admin pages
    /// </summary>
    [HttpGet("check-admin")]
    [Authorize]
    public async Task<IActionResult> CheckAdmin()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var isAdmin = await authService.IsUserAdmin(userId);
            return Ok(new CheckAdminResponse(isAdmin));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking admin status");
            return StatusCode(500, new { message = "An error occurred while checking admin status" });
        }
    }

    /// <summary>
    /// Get current user information including roles and firm details
    /// Use this for fresh user data or after important operations
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserInfo()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var userInfo = await authService.GetUserInfo(userId);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user info");
            return StatusCode(500, new { message = "An error occurred while getting user info" });
        }
    }
}
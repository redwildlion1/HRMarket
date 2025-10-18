using FluentValidation;
using HRMarket.Core.Auth.Examples;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace HRMarket.Core.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
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
        await authService.ConfirmEmail(userId, token);
        return Ok();
    }
}

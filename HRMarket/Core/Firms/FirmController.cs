using HRMarket.Core.Firms.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMarket.Core.Firms;

[ApiController]
[Route("api/firms")]
public class FirmController(
    IFirmService service,
    IFirmAuthorizationService authorizationService,
    ILogger<FirmController> logger) : ControllerBase
{
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateFirmDto dto)
    {
        try
        {
            var result = await service.CreateAsync(dto);
            return Ok(new { firmId = result, message = "Firm created successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating firm");
            return StatusCode(500, new { message = "An error occurred while creating the firm" });
        }
    }

    /// <summary>
    /// Check if the current user can edit the specified firm
    /// Returns: canEdit (true if admin or owner), isFirmOwner (true only if owner)
    /// </summary>
    [HttpGet("{firmId:guid}/can-edit")]
    [Authorize]
    public async Task<IActionResult> CheckFirmAccess([FromRoute] Guid firmId)
    {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await authorizationService.CanUserEditFirm(userId, firmId);
            return Ok(result);
    }

    /// <summary>
    /// Get the firm ID associated with the current user
    /// </summary>
    [HttpGet("my-firm")]
    [Authorize]
    public async Task<IActionResult> GetMyFirm()
    {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var firmId = await authorizationService.GetUserFirmId(userId);
            
            if (firmId == null)
            {
                return NotFound(new { message = "No firm associated with this user" });
            }

            return Ok(new { firmId });
    }
}
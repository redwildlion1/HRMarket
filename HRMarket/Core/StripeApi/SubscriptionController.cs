using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.StripeApi;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController(
    ISubscriptionService subscriptionService,
    ILogger<SubscriptionController> logger)
    : ControllerBase
{
    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans()
    {
        var plans = await subscriptionService.GetAllPlansAsync();
        return Ok(plans);
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan(
        [FromBody] CreateSubscriptionPlanDto dto)
    {
        var plan = await subscriptionService.CreatePlanAsync(dto);
        return CreatedAtAction(nameof(GetPlans), new { id = plan.Id }, plan);
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var firmId = Guid.Parse(User.FindFirstValue("FirmId")!);

        if (dto.FirmId != firmId)
            return Forbid();

        var response = await subscriptionService.CreateCheckoutSessionAsync(dto, userId);
        return Ok(response);
    }

    [HttpGet("firms/{firmId:guid}/status")]
    [Authorize]
    public async Task<ActionResult<SubscriptionStatusDto>> GetFirmSubscriptionStatus(Guid firmId)
    {
        var userFirmId = Guid.Parse(User.FindFirstValue("FirmId")!);
        if (firmId != userFirmId)
            return Forbid();

        var status = await subscriptionService.GetFirmSubscriptionStatusAsync(firmId);
        if (status == null)
            return NotFound();

        return Ok(status);
    }
}
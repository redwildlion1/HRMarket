using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.OuterAPIs.StripeApi;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController(
    ISubscriptionService subscriptionService,
    ILogger<SubscriptionController> logger)
    : ControllerBase
{
    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDTO>>> GetPlans()
    {
        var plans = await subscriptionService.GetAllPlansAsync();
        return Ok(plans);
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubscriptionPlanDTO>> CreatePlan(
        [FromBody] CreateSubscriptionPlanDTO dto)
    {
        var plan = await subscriptionService.CreatePlanAsync(dto);
        return CreatedAtAction(nameof(GetPlans), new { id = plan.Id }, plan);
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionDTO dto)
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
    public async Task<ActionResult<SubscriptionStatusDTO>> GetFirmSubscriptionStatus(Guid firmId)
    {
        var userFirmId = Guid.Parse(User.FindFirstValue("FirmId")!);
        if (firmId != userFirmId)
            return Forbid();

        var status = await subscriptionService.GetFirmSubscriptionStatusAsync(firmId);
        if (status == null)
            return NotFound();

        return Ok(status);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await subscriptionService.HandleStripeWebhookAsync(json, signature);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook handling failed");
            return BadRequest();
        }
    }
}
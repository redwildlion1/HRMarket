using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.OuterAPIs.StripeWebhook;

public class WebhookController(
    IWebhookService webhookService,
    ILogger<WebhookController> logger) : ControllerBase
{
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await webhookService.HandleStripeWebhookAsync(json, signature);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook handling failed");
            return BadRequest();
        }
    }
}
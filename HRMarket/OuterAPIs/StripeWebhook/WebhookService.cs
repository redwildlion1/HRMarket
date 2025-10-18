using HRMarket.Configuration.Settings;
using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;
using HRMarket.Core.StripeApi;
using HRMarket.Entities.Stripe;
using Stripe;
using Stripe.Checkout;

namespace HRMarket.OuterAPIs.StripeWebhook;

public interface IWebhookService
{
    Task HandleStripeWebhookAsync(string json, string signature);
}

public class WebhookService(
    ISubscriptionRepository repository,
    StripeSettings stripeSettings,
    ILogger<WebhookService> logger)
    : IWebhookService
{
    public async Task HandleStripeWebhookAsync(string json, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                stripeSettings.WebhookSecret
            );

            logger.LogInformation("Processing Stripe webhook: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case StripeEventTypes.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;

                case StripeEventTypes.CustomerSubscriptionCreated:
                case StripeEventTypes.CustomerSubscriptionUpdated:
                    await HandleSubscriptionUpdatedAsync(stripeEvent);
                    break;

                case StripeEventTypes.CustomerSubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync(stripeEvent);
                    break;

                case StripeEventTypes.InvoicePaid:
                    await HandleInvoicePaidAsync(stripeEvent);
                    break;

                case StripeEventTypes.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(stripeEvent);
                    break;

                default:
                    logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook");
            throw;
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Session session) return;

        var firmId = Guid.Parse(session.Metadata["firmid"]);
        var isYearly = bool.Parse(session.Metadata["isyearly"]);

        // The subscription ID should be in the session
        var subscriptionId = session.Subscription.Id;
        if (string.IsNullOrEmpty(subscriptionId))
        {
            logger.LogError("Subscription ID is missing");
            throw new InvalidOperationException("No subscription ID found in checkout session");
        }

        // Get subscription details from Stripe
        var subscriptionService = new Stripe.SubscriptionService();
        var stripeSubscription = await subscriptionService.GetAsync(subscriptionId);

        // Get plan from price ID
        var priceId = stripeSubscription.Items.Data[0].Price.Id;
        var plans = await repository.GetAllActivePlansAsync();
        var plan = plans.FirstOrDefault(p =>
            p.StripePriceIdMonthly == priceId || p.StripePriceIdYearly == priceId);

        if (plan == null)
        {
            logger.LogError("No plan found for price ID: {PriceId}", priceId);
            throw new InvalidOperationException($"No plan found for price ID {priceId}");
        }

        // Create firm subscription
        var firmSubscription = new FirmSubscription
        {
            FirmId = firmId,
            SubscriptionPlanId = plan.Id,
            StripeSubscriptionId = stripeSubscription.Id,
            StripeCustomerId = stripeSubscription.CustomerId,
            Status = stripeSubscription.Status.MapStripeStatus(),
            IsYearly = isYearly,
            CurrentPeriodStart = stripeSubscription.Items.First().CurrentPeriodStart,
            CurrentPeriodEnd = stripeSubscription.Items.First().CurrentPeriodEnd,
        };

        await repository.CreateFirmSubscriptionAsync(firmSubscription);
        logger.LogInformation("Created subscription for firm {FirmId}", firmId);
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Subscription stripeSubscription) return;

        var subscription = await repository.GetSubscriptionByStripeIdAsync(stripeSubscription.Id);
        if (subscription == null)
        {
            logger.LogError("Subscription not found for Stripe ID: {StripeId}", stripeSubscription.Id);
            throw new InvalidOperationException($"Subscription not found for Stripe ID {stripeSubscription.Id}");
        }

        subscription.Status = stripeSubscription.Status.MapStripeStatus();
        subscription.CurrentPeriodStart = stripeSubscription.Items.First().CurrentPeriodStart;
        subscription.CurrentPeriodEnd = stripeSubscription.Items.First().CurrentPeriodEnd;
        subscription.CancelAt = stripeSubscription.CancelAt;

        await repository.UpdateFirmSubscriptionAsync(subscription);
        logger.LogInformation("Updated subscription {SubscriptionId}", subscription.Id);
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Subscription stripeSubscription) return;

        var subscription = await repository.GetSubscriptionByStripeIdAsync(stripeSubscription.Id);
        if (subscription == null)
        {
            logger.LogError("Subscription not found for Stripe ID: {StripeId}", stripeSubscription.Id);
            throw new InvalidOperationException($"Subscription not found for Stripe ID {stripeSubscription.Id}");
        }

        subscription.Status = SubscriptionStatus.Canceled;
        subscription.CanceledAt = DateTime.UtcNow;

        await repository.UpdateFirmSubscriptionAsync(subscription);
        logger.LogInformation("Canceled subscription {SubscriptionId}", subscription.Id);
    }

    private async Task HandleInvoicePaidAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Invoice invoice) return;

        var subscriptionId =
            invoice.Parent.SubscriptionDetails.SubscriptionId
            ?? invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId
            ?? throw new InvalidOperationException("No subscription ID found on invoice");

        if (string.IsNullOrEmpty(subscriptionId))
        {
            logger.LogError("Subscription ID is missing on invoice {InvoiceId}", invoice.Id);
            throw new InvalidOperationException("No subscription ID found on invoice");
        }

        var subscription = await repository.GetSubscriptionByStripeIdAsync(subscriptionId);
        if (subscription == null)
        {
            logger.LogWarning("Subscription not found for Stripe ID: {StripeId}", subscriptionId);
            return;
        }

        // Get payment intent ID - it might be null for some invoice types
        var paymentIntentId = invoice.Payments.First().Payment.PaymentIntentId;

        var payment = new PaymentHistory
        {
            FirmId = subscription.FirmId,
            StripeInvoiceId = invoice.Id,
            StripePaymentIntentId = paymentIntentId,
            Amount = (decimal)(invoice.AmountPaid) / 100,
            Currency = invoice.Currency ?? "ron",
            Status = PaymentStatus.Paid,
            PaidAt = invoice.StatusTransitions?.PaidAt ?? DateTime.UtcNow
        };

        await repository.CreatePaymentHistoryAsync(payment);
        logger.LogInformation("Recorded payment for firm {FirmId}, amount {Amount} {Currency}",
            subscription.FirmId, payment.Amount, payment.Currency);
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Invoice invoice) return;

        // Get subscription ID from invoice lines or subscription property
        var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId
                             ?? invoice.Parent.SubscriptionDetails.SubscriptionId
                             ?? throw new InvalidOperationException("No subscription ID found on invoice");

        var subscription = await repository.GetSubscriptionByStripeIdAsync(subscriptionId);
        if (subscription == null)
        {
            logger.LogError("Subscription not found for Stripe ID: {StripeId}", subscriptionId);
            throw new InvalidOperationException($"Subscription not found for Stripe ID {subscriptionId}");
        }

        subscription.Status = SubscriptionStatus.PastDue;
        await repository.UpdateFirmSubscriptionAsync(subscription);

        logger.LogWarning("Payment failed for firm {FirmId}, invoice {InvoiceId}",
            subscription.FirmId, invoice.Id);
    }
}
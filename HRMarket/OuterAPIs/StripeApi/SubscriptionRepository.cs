using HRMarket.Entities;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace HRMarket.OuterAPIs.StripeApi;

public interface ISubscriptionRepository
{
    Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId);
    Task<List<SubscriptionPlan>> GetAllActivePlansAsync();
    Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan);
    Task UpdatePlanAsync(SubscriptionPlan plan);
    Task<FirmSubscription?> GetActiveSubscriptionByFirmIdAsync(Guid firmId);
    Task<FirmSubscription?> GetSubscriptionByStripeIdAsync(string stripeSubscriptionId);
    Task<FirmSubscription> CreateFirmSubscriptionAsync(FirmSubscription subscription);
    Task UpdateFirmSubscriptionAsync(FirmSubscription subscription);
    Task<PaymentHistory> CreatePaymentHistoryAsync(PaymentHistory payment);
    Task<List<PaymentHistory>> GetPaymentHistoryByFirmIdAsync(Guid firmId);
}

public class SubscriptionRepository(ApplicationDbContext context) : ISubscriptionRepository
{
    public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId)
    {
        return await context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(sp => sp.Id == planId && sp.IsActive);
    }

    public async Task<List<SubscriptionPlan>> GetAllActivePlansAsync()
    {
        return await context.Set<SubscriptionPlan>()
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.PriceMonthly)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan)
    {
        context.Set<SubscriptionPlan>().Add(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public async Task UpdatePlanAsync(SubscriptionPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<FirmSubscription?> GetActiveSubscriptionByFirmIdAsync(Guid firmId)
    {
        return await context.Set<FirmSubscription>()
            .Include(fs => fs.SubscriptionPlan)
            .FirstOrDefaultAsync(fs => 
                fs.FirmId == firmId && 
                (fs.Status == SubscriptionStatus.Active || fs.Status == SubscriptionStatus.Trialing));
    }

    public async Task<FirmSubscription?> GetSubscriptionByStripeIdAsync(string stripeSubscriptionId)
    {
        return await context.Set<FirmSubscription>()
            .Include(fs => fs.SubscriptionPlan)
            .Include(fs => fs.Firm)
            .FirstOrDefaultAsync(fs => fs.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<FirmSubscription> CreateFirmSubscriptionAsync(FirmSubscription subscription)
    {
        context.Set<FirmSubscription>().Add(subscription);
        await context.SaveChangesAsync();
        return subscription;
    }

    public async Task UpdateFirmSubscriptionAsync(FirmSubscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<PaymentHistory> CreatePaymentHistoryAsync(PaymentHistory payment)
    {
        context.Set<PaymentHistory>().Add(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    public async Task<List<PaymentHistory>> GetPaymentHistoryByFirmIdAsync(Guid firmId)
    {
        return await context.Set<PaymentHistory>()
            .Where(ph => ph.FirmId == firmId)
            .OrderByDescending(ph => ph.CreatedAt)
            .ToListAsync();
    }
}

// Service
public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDTO>> GetAllPlansAsync();
    Task<SubscriptionPlanDTO> CreatePlanAsync(CreateSubscriptionPlanDTO dto);
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionDTO dto, Guid userId);
    Task<SubscriptionStatusDTO?> GetFirmSubscriptionStatusAsync(Guid firmId);
    Task HandleStripeWebhookAsync(string json, string signature);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ISubscriptionRepository repository,
        StripeSettings stripeSettings,
        ILogger<SubscriptionService> logger)
    {
        _repository = repository;
        _stripeSettings = stripeSettings;
        _logger = logger;
        StripeConfiguration.ApiKey = stripeSettings.SecretKey;
    }

    public async Task<List<SubscriptionPlanDTO>> GetAllPlansAsync()
    {
        var plans = await _repository.GetAllActivePlansAsync();
        return plans.Select(MapToDTO).ToList();
    }

    public async Task<SubscriptionPlanDTO> CreatePlanAsync(CreateSubscriptionPlanDTO dto)
    {
        // Create product in Stripe
        var productService = new ProductService();
        var product = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = dto.Name,
            Description = dto.Description
        });

        // Create monthly price
        var priceService = new PriceService();
        var monthlyPrice = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmount = (long)(dto.PriceMonthly * 100),
            Currency = "usd",
            Recurring = new PriceRecurringOptions
            {
                Interval = "month"
            }
        });

        // Create yearly price
        var yearlyPrice = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmount = (long)(dto.PriceYearly * 100),
            Currency = "usd",
            Recurring = new PriceRecurringOptions
            {
                Interval = "year"
            }
        });

        // Save to database
        var plan = new SubscriptionPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            PriceMonthly = dto.PriceMonthly,
            PriceYearly = dto.PriceYearly,
            StripePriceIdMonthly = monthlyPrice.Id,
            StripePriceIdYearly = yearlyPrice.Id,
            StripeProductId = product.Id,
            Features = dto.Features,
            IsPopular = dto.IsPopular
        };

        await _repository.CreatePlanAsync(plan);
        return MapToDTO(plan);
    }

    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
        CreateCheckoutSessionDTO dto, 
        Guid userId)
    {
        var plan = await _repository.GetPlanByIdAsync(dto.FirmId);
        if (plan == null)
            throw new ArgumentException("Subscription plan not found");

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = dto.PriceId,
                    Quantity = 1
                }
            },
            Mode = "subscription",
            SuccessUrl = $"{_stripeSettings.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = _stripeSettings.CancelUrl,
            ClientReferenceId = dto.FirmId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                { "firm_id", dto.FirmId.ToString() },
                { "user_id", userId.ToString() },
                { "is_yearly", dto.IsYearly.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return new CheckoutSessionResponse(session.Id, _stripeSettings.PublishableKey);
    }

    public async Task<SubscriptionStatusDTO?> GetFirmSubscriptionStatusAsync(Guid firmId)
    {
        var subscription = await _repository.GetActiveSubscriptionByFirmIdAsync(firmId);
        if (subscription == null)
            return null;

        return new SubscriptionStatusDTO(
            subscription.Id,
            subscription.Status,
            subscription.CurrentPeriodEnd,
            subscription.IsYearly,
            MapToDTO(subscription.SubscriptionPlan)
        );
    }

    public async Task HandleStripeWebhookAsync(string json, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _stripeSettings.WebhookSecret
            );

            _logger.LogInformation("Processing Stripe webhook: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;
                    
                case Events.CustomerSubscriptionCreated:
                case Events.CustomerSubscriptionUpdated:
                    await HandleSubscriptionUpdatedAsync(stripeEvent);
                    break;
                    
                case Events.CustomerSubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync(stripeEvent);
                    break;
                    
                case Events.InvoicePaid:
                    await HandleInvoicePaidAsync(stripeEvent);
                    break;
                    
                case Events.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(stripeEvent);
                    break;
                    
                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            throw;
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Session session) return;

        var firmId = Guid.Parse(session.Metadata["firm_id"]);
        var isYearly = bool.Parse(session.Metadata["is_yearly"]);

        // Get subscription details from Stripe
        var subscriptionService = new Stripe.SubscriptionService();
        var stripeSubscription = await subscriptionService.GetAsync(session.SubscriptionId);

        // Get plan from price ID
        var priceId = stripeSubscription.Items.Data[0].Price.Id;
        var plans = await _repository.GetAllActivePlansAsync();
        var plan = plans.FirstOrDefault(p => 
            p.StripePriceIdMonthly == priceId || p.StripePriceIdYearly == priceId);

        if (plan == null)
        {
            _logger.LogError("Could not find plan for price ID: {PriceId}", priceId);
            return;
        }

        // Create firm subscription
        var firmSubscription = new FirmSubscription
        {
            FirmId = firmId,
            SubscriptionPlanId = plan.Id,
            StripeSubscriptionId = stripeSubscription.Id,
            StripeCustomerId = stripeSubscription.CustomerId,
            Status = MapStripeStatus(stripeSubscription.Status),
            IsYearly = isYearly,
            CurrentPeriodStart = stripeSubscription.StartDate,
            CurrentPeriodEnd = stripeSubscription.NextPendingInvoiceItemInvoice ?? DateTime.MaxValue
        };

        await _repository.CreateFirmSubscriptionAsync(firmSubscription);
        _logger.LogInformation("Created subscription for firm {FirmId}", firmId);
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Subscription stripeSubscription) return;

        var subscription = await _repository.GetSubscriptionByStripeIdAsync(stripeSubscription.Id);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for Stripe ID: {StripeId}", stripeSubscription.Id);
            return;
        }

        subscription.Status = MapStripeStatus(stripeSubscription.Status);
        subscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
        subscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
        subscription.CancelAt = stripeSubscription.CancelAt;

        await _repository.UpdateFirmSubscriptionAsync(subscription);
        _logger.LogInformation("Updated subscription {SubscriptionId}", subscription.Id);
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Subscription stripeSubscription) return;

        var subscription = await _repository.GetSubscriptionByStripeIdAsync(stripeSubscription.Id);
        if (subscription == null) return;

        subscription.Status = SubscriptionStatus.Canceled;
        subscription.CanceledAt = DateTime.UtcNow;

        await _repository.UpdateFirmSubscriptionAsync(subscription);
        _logger.LogInformation("Canceled subscription {SubscriptionId}", subscription.Id);
    }

    private async Task HandleInvoicePaidAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Invoice invoice) return;

        var subscription = await _repository.GetSubscriptionByStripeIdAsync(invoice.SubscriptionId);
        if (subscription == null) return;

        var payment = new PaymentHistory
        {
            FirmId = subscription.FirmId,
            StripeInvoiceId = invoice.Id,
            StripePaymentIntentId = invoice.PaymentIntentId,
            Amount = (decimal)invoice.AmountPaid / 100,
            Currency = invoice.Currency,
            Status = PaymentStatus.Paid,
            PaidAt = invoice.StatusTransitions.PaidAt ?? DateTime.UtcNow
        };

        await _repository.CreatePaymentHistoryAsync(payment);
        _logger.LogInformation("Recorded payment for firm {FirmId}", subscription.FirmId);
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var subscription = await _repository.GetSubscriptionByStripeIdAsync(invoice.SubscriptionId);
        if (subscription == null) return;

        subscription.Status = SubscriptionStatus.PastDue;
        await _repository.UpdateFirmSubscriptionAsync(subscription);
        
        _logger.LogWarning("Payment failed for firm {FirmId}", subscription.FirmId);
    }

    private static SubscriptionPlanDTO MapToDTO(SubscriptionPlan plan)
    {
        return new SubscriptionPlanDTO(
            plan.Id,
            plan.Name,
            plan.Description,
            plan.PriceMonthly,
            plan.PriceYearly,
            plan.StripePriceIdMonthly,
            plan.StripePriceIdYearly,
            plan.IsPopular,
            plan.Features
        );
    }

    private static SubscriptionStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            "trialing" => SubscriptionStatus.Trialing,
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Canceled,
            "unpaid" => SubscriptionStatus.Unpaid,
            _ => SubscriptionStatus.Canceled
        };
    }
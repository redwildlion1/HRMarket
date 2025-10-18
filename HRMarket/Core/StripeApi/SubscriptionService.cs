using HRMarket.Configuration.Settings;
using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;
using HRMarket.Entities.Stripe;
using Stripe;
using Stripe.Checkout;

namespace HRMarket.Core.StripeApi;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetAllPlansAsync();
    Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto);
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionDto dto, Guid userId);
    Task<SubscriptionStatusDto?> GetFirmSubscriptionStatusAsync(Guid firmId);
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

    public async Task<List<SubscriptionPlanDto>> GetAllPlansAsync()
    {
        var plans = await _repository.GetAllActivePlansAsync();
        return plans.Select(p => p.MapToDto()).ToList();
    }

    public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto)
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
            Currency = "ron",
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
            Currency = "ron",
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
        return plan.MapToDto();
    }

    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
        CreateCheckoutSessionDto dto,
        Guid userId)
    {
        var plan = await _repository.GetPlanByIdAsync(dto.FirmId);
        if (plan == null)
        {
            _logger.LogWarning("Plan with ID {PlanId} not found.", dto.FirmId);
            throw new Exception("Subscription plan not found.");
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = dto.PriceId,
                    Quantity = 1
                }
            ],
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

    public async Task<SubscriptionStatusDto?> GetFirmSubscriptionStatusAsync(Guid firmId)
    {
        var subscription = await _repository.GetActiveSubscriptionByFirmIdAsync(firmId);
        if (subscription == null)
            return null;

        return new SubscriptionStatusDto(
            subscription.Id,
            subscription.Status,
            subscription.CurrentPeriodEnd,
            subscription.IsYearly,
            subscription.SubscriptionPlan.MapToDto()
        );
    }
}
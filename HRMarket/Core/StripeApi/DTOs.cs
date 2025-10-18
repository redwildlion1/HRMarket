// HRMarket/Core/StripeApi/DTOs.cs

using HRMarket.Configuration.Status;
using HRMarket.Entities.Stripe;

namespace HRMarket.Core.StripeApi;

public class SubscriptionPlanDto(
    Guid id,
    string name,
    string description,
    decimal priceMonthly,
    decimal priceYearly,
    string stripePriceIdMonthly,
    string stripePriceIdYearly,
    bool isPopular,
    List<string> features
)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public decimal PriceMonthly { get; } = priceMonthly;
    public decimal PriceYearly { get; } = priceYearly;
    public string StripePriceIdMonthly { get; } = stripePriceIdMonthly;
    public string StripePriceIdYearly { get; } = stripePriceIdYearly;
    public bool IsPopular { get; } = isPopular;
    public List<string> Features { get; } = features;
}

public class CreateSubscriptionPlanDto(
    string name,
    string description,
    decimal priceMonthly,
    decimal priceYearly,
    List<string> features,
    bool isPopular = false
) : BaseDto
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public decimal PriceMonthly { get; } = priceMonthly;
    public decimal PriceYearly { get; } = priceYearly;
    public List<string> Features { get; } = features;
    public bool IsPopular { get; } = isPopular;
}

public class CreateCheckoutSessionDto(
    Guid firmId,
    string priceId,
    bool isYearly
) : BaseDto
{
    public Guid FirmId { get; } = firmId;
    public string PriceId { get; } = priceId;
    public bool IsYearly { get; } = isYearly;
}

public class CheckoutSessionResponse(
    string sessionId,
    string publishableKey
) : BaseDto
{
    public string SessionId { get; } = sessionId;
    public string PublishableKey { get; } = publishableKey;
}

public class SubscriptionStatusDto(
    Guid subscriptionId,
    SubscriptionStatus status,
    DateTime currentPeriodEnd,
    bool isYearly,
    SubscriptionPlanDto plan
);

public static class SubscriptionStatusExtensions
{
    public static SubscriptionPlanDto MapToDto(this SubscriptionPlan plan)
    {
        return new SubscriptionPlanDto(
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

    public static SubscriptionStatus MapStripeStatus(this string stripeStatus)
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
}
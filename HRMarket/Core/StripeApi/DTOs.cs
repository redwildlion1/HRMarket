using HRMarket.Configuration.Status;
using HRMarket.Entities.Stripe;

namespace HRMarket.Core.StripeApi;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    string StripePriceIdMonthly,
    string StripePriceIdYearly,
    bool IsPopular,
    List<string> Features
);

public record CreateSubscriptionPlanDto(
    string Name,
    string Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    List<string> Features,
    bool IsPopular = false
);

public record CreateCheckoutSessionDto(
    Guid FirmId,
    string PriceId,
    bool IsYearly
);

public record CheckoutSessionResponse(
    string SessionId,
    string PublishableKey
);

public record SubscriptionStatusDto(
    Guid SubscriptionId,
    SubscriptionStatus Status,
    DateTime CurrentPeriodEnd,
    bool IsYearly,
    SubscriptionPlanDto Plan
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
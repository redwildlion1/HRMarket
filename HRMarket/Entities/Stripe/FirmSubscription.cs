using HRMarket.Configuration.Status;
using HRMarket.Entities.Firms;

namespace HRMarket.Entities.Stripe;

public class FirmSubscription
{
    public Guid Id { get; set; }
    public Guid FirmId { get; set; }
    public Firm Firm { get; set; } = null!;

    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    public required string StripeSubscriptionId { get; set; }
    public required string StripeCustomerId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool IsYearly { get; set; }

    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
    public DateTime? CancelAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
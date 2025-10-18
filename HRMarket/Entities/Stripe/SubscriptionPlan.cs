namespace HRMarket.Entities.Stripe;

public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }
    public required string StripePriceIdMonthly { get; set; }
    public required string StripePriceIdYearly { get; set; }
    public required string StripeProductId { get; set; }
    public bool IsPopular { get; set; }
    public int MaxListings { get; set; }
    public List<string> Features { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<FirmSubscription> FirmSubscriptions { get; set; } = new();
}
using HRMarket.Entities.Firms;

namespace HRMarket.OuterAPIs.StripeApi;

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

// Firm Subscription Entity
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


// Payment History Entity
public class PaymentHistory
{
    public Guid Id { get; set; }
    public Guid FirmId { get; set; }
    public Firm Firm { get; set; } = null!;
    
    public required string StripeInvoiceId { get; set; }
    public required string StripePaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


// Entity Configuration
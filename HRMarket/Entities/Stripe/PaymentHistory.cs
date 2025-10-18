using HRMarket.Configuration.Status;
using HRMarket.Entities.Firms;

namespace HRMarket.Entities.Stripe;

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
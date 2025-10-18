namespace HRMarket.Configuration.Types;

/// <summary>
/// Constants for Stripe webhook event types
/// </summary>
public static class StripeEventTypes
{
    // Checkout events
    public const string CheckoutSessionCompleted = "checkout.session.completed";
    public const string CheckoutSessionExpired = "checkout.session.expired";

    // Customer events
    public const string CustomerCreated = "customer.created";
    public const string CustomerUpdated = "customer.updated";
    public const string CustomerDeleted = "customer.deleted";

    // Subscription events
    public const string CustomerSubscriptionCreated = "customer.subscription.created";
    public const string CustomerSubscriptionUpdated = "customer.subscription.updated";
    public const string CustomerSubscriptionDeleted = "customer.subscription.deleted";
    public const string CustomerSubscriptionTrialWillEnd = "customer.subscription.trial_will_end";

    // Invoice events
    public const string InvoiceCreated = "invoice.created";
    public const string InvoiceFinalized = "invoice.finalized";
    public const string InvoicePaid = "invoice.paid";
    public const string InvoicePaymentFailed = "invoice.payment_failed";
    public const string InvoicePaymentSucceeded = "invoice.payment_succeeded";
    public const string InvoiceUpcoming = "invoice.upcoming";

    // Payment events
    public const string PaymentIntentSucceeded = "payment_intent.succeeded";
    public const string PaymentIntentPaymentFailed = "payment_intent.payment_failed";
    public const string PaymentIntentCreated = "payment_intent.created";

    // Charge events
    public const string ChargeSucceeded = "charge.succeeded";
    public const string ChargeFailed = "charge.failed";
    public const string ChargeRefunded = "charge.refunded";
}
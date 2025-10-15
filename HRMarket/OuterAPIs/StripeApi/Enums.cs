namespace HRMarket.OuterAPIs.StripeApi;

public enum SubscriptionStatus
{
    Incomplete,
    IncompleteExpired,
    Trialing,
    Active,
    PastDue,
    Canceled,
    Unpaid
}

public enum PaymentStatus
{
    Draft,
    Open,
    Paid,
    Uncollectible,
    Void
}
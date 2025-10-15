namespace HRMarket.OuterAPIs.StripeApi;

public record SubscriptionPlanDTO(
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

public record CreateSubscriptionPlanDTO(
    string Name,
    string Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    List<string> Features,
    bool IsPopular = false
);

public record CreateCheckoutSessionDTO(
    Guid FirmId,
    string PriceId,
    bool IsYearly
);

public record CheckoutSessionResponse(
    string SessionId,
    string PublishableKey
);

public record SubscriptionStatusDTO(
    Guid SubscriptionId,
    SubscriptionStatus Status,
    DateTime CurrentPeriodEnd,
    bool IsYearly,
    SubscriptionPlanDTO Plan
);
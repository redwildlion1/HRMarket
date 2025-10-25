namespace HRMarket.Configuration.Status;

public enum FirmStatus
{
    Draft,              // Firm created but incomplete
    AwaitingReview,     // All required steps completed, waiting for admin approval
    Approved,           // Admin approved
    Rejected,           // Admin rejected
    Active,             // Approved and actively using the platform
    Suspended           // Temporarily suspended
}

public enum FirmRejectionReason
{
    InappropriateContent,
    FalseInformation,
    MissingDocumentation,
    ViolatesTerms,
    Other
}
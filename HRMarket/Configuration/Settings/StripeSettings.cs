namespace HRMarket.Configuration.Settings;

public class StripeSettings
{
    public const string SectionName = "StripeSettings";
    public required string SecretKey { get; set; }
    public required string PublishableKey { get; set; }
    public required string WebhookSecret { get; set; }
    public required string SuccessUrl { get; set; }
    public required string CancelUrl { get; set; }
}
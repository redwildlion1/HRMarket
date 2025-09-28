namespace HRMarket.Core.Auth.Tokens;

public class TokenSettings
{
    public const string SectionName = "TokenSettings";
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SecretKey { get; set; }
    public int JwtExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}
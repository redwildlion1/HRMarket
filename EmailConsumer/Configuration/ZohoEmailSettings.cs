namespace EmailConsumer.Configuration;

public class ZohoEmailSettings
{
    public required string SmtpServer { get; init; } = "smtp.zoho.com";
    public int SmtpPort { get; init; } = 587;
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
    public bool EnableSsl { get; init; } = true;
}

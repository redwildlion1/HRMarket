using Common.Email;
using MassTransit;

namespace HRMarket.OuterAPIs.Email;

public class EmailProducer(IPublishEndpoint publishEndpoint, ILogger<EmailProducer> logger)
{
    public async Task ProduceEmailAsync(EmailMessage emailMessage)
    {
        try
        {
            await publishEndpoint.Publish(emailMessage);
            logger.LogInformation("Email message sent for: {ToEmail}", emailMessage.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish email message");
            throw;
        }
    }
}
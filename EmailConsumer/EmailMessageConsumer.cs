using Common.Email;
using EmailConsumer.Services;
using MassTransit;

namespace EmailConsumer;

public class EmailMessageConsumer(IEmailService emailService, ILogger<EmailMessageConsumer> logger)
    : IConsumer<EmailMessage>
{
    public async Task Consume(ConsumeContext<EmailMessage> context)
    {
        try
        {
            var message = context.Message;
            await emailService.SendEmailAsync(message);
            logger.LogInformation("Processed email message for: {ToEmail}", message.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing email message");
            throw; // Rethrow to trigger retry mechanisms if configured
        }
    }
}
using Common.Email;

namespace HRMarket.OuterAPIs.Email;

public interface IEmailService
{
    Task SendConfirmationEmail(string toEmail, string confirmationLink);
}

// This is a Kafka producer that sends email messages to a Kafka topic
public class EmailService(EmailProducer emailProducer) : IEmailService
{
    public async Task SendConfirmationEmail(string toEmail, string confirmationLink)
    {
        var htmlBody = EmailTemplates.GetConfirmationEmailBody(confirmationLink);
        var subject = EmailTemplates.ConfirmationEmailSubject;
        
        var emailMessage = new EmailMessage
        {
            ToEmail = toEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        
        await emailProducer.ProduceEmailAsync(emailMessage);
    }
}
using Common.Email;

namespace EmailConsumer.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage emailMessage);
}

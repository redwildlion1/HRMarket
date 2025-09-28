using Common.Email;
using EmailConsumer.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailConsumer.Services;

public class EmailService(ILogger<EmailService> logger, IOptions<ZohoEmailSettings> zohoSettings)
    : IEmailService, IDisposable
{
    private readonly ZohoEmailSettings _zohoSettings = zohoSettings.Value;
    private SmtpClient? _smtpClient;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private DateTime _lastUsed = DateTime.UtcNow;
    private readonly TimeSpan _connectionTimeout = TimeSpan.FromMinutes(5);

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            await EnsureConnectedAsync();
            
            var message = CreateMimeMessage(emailMessage);
            await _smtpClient!.SendAsync(message);
            
            _lastUsed = DateTime.UtcNow;
            logger.LogInformation("Email sent successfully to: {ToEmail}", emailMessage.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to: {ToEmail}", emailMessage.ToEmail);
            await DisconnectAsync();
            throw;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_smtpClient == null || !_smtpClient.IsConnected || 
            DateTime.UtcNow - _lastUsed > _connectionTimeout)
        {
            await DisconnectAsync();
            
            _smtpClient = new SmtpClient();
            await _smtpClient.ConnectAsync(_zohoSettings.SmtpServer, _zohoSettings.SmtpPort, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(_zohoSettings.Username, _zohoSettings.Password);
            
            logger.LogInformation("Connected to Zoho SMTP server");
        }
    }

    private MimeMessage CreateMimeMessage(EmailMessage emailMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_zohoSettings.FromName, _zohoSettings.FromEmail));
        message.To.Add(new MailboxAddress("", emailMessage.ToEmail));
        message.Subject = emailMessage.Subject;
        message.Body = new BodyBuilder { HtmlBody = emailMessage.HtmlBody }.ToMessageBody();
        return message;
    }

    private async Task DisconnectAsync()
    {
        if (_smtpClient?.IsConnected == true)
        {
            await _smtpClient.DisconnectAsync(true);
        }
        _smtpClient?.Dispose();
        _smtpClient = null;
    }

    public void Dispose()
    {
        _connectionSemaphore.Wait();
        try
        {
            DisconnectAsync().GetAwaiter().GetResult();
            _connectionSemaphore.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing EmailService");
        }
    }
}

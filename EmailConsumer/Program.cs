using Common.Email;
using EmailConsumer;
using EmailConsumer.Configuration;
using EmailConsumer.Services;
using MassTransit;


var builder = Host.CreateApplicationBuilder(args);

var sharedConfigPath = Path.Combine(builder.Environment.ContentRootPath, "..", "Common", "CommonSettings.json");
builder.Configuration.AddJsonFile(sharedConfigPath, optional: false, reloadOnChange: true);

builder.Services.Configure<EmailQueueSettings>(
    builder.Configuration.GetSection(EmailQueueSettings.SectionName));

// Configure ZohoEmailSettings from appsettings.json
builder.Services.Configure<ZohoEmailSettings>(
    builder.Configuration.GetSection("ZohoEmailSettings"));

// Register services
builder.Services.AddScoped<IEmailService, EmailService>();

// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    // Register the consumer
    x.AddConsumer<EmailMessageConsumer>();

    // Configure RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
        var emailSettings = builder.Configuration.GetSection(EmailQueueSettings.SectionName).Get<EmailQueueSettings>();

        if (emailSettings == null) throw new InvalidOperationException("EmailSettings section is missing in configuration.");
        
        cfg.Host(emailSettings.Host, h =>
        {
            h.Username(emailSettings.Username);
            h.Password(emailSettings.Password);
        });

        cfg.ReceiveEndpoint(emailSettings.QueueName, e =>
        {
            e.ConfigureConsumer<EmailMessageConsumer>(context);
            e.PrefetchCount = 10; // optional: number of unprocessed messages in memory
        });
    });
});

var host = builder.Build();
await host.RunAsync();
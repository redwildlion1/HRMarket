using Common.AWS;
using Common.Media;
using FileUploadConsumer;
using FileUploadConsumer.Antivirus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // AWS S3 Configuration
        var awsSettings = context.Configuration.GetSection(AwsSettings.Section).Get<AwsSettings>();
        services.AddScoped<IClamScanner, ClamScanner>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton(new AwsConfigurator(awsSettings!));
        
        // MassTransit setup
        services.AddMassTransit(x =>
        {
            x.AddConsumer<FileUploadedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("admin");
                    h.Password("L@urentiu2003");
                });

                cfg.ReceiveEndpoint("file-uploaded-queue", e =>
                {
                    e.ConfigureConsumer<FileUploadedConsumer>(ctx);
                });
            });
        });
        
    })
    .Build()
    .Run();
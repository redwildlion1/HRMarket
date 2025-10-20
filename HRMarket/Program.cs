using System.Security.Claims;
using System.Text;
using Amazon.S3;
using Common.AWS;
using Common.Email;
using Common.Media;
using FluentValidation;
using HRMarket.Configuration.Exceptions;
using HRMarket.Configuration.Swagger;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Answers;
using HRMarket.Core.Auth;
using HRMarket.Core.Auth.Extensions;
using HRMarket.Core.Auth.Tokens;
using HRMarket.Core.Categories;
using HRMarket.Core.Firms;
using HRMarket.Core.Firms.DTOs;
using HRMarket.Core.Media;
using HRMarket.Core.Questions;
using HRMarket.Entities;
using HRMarket.Entities.Auth;
using HRMarket.Middleware;
using HRMarket.OuterAPIs.Email;
using HRMarket.OuterAPIs.Media;
using HRMarket.Validation.Extensions;
using HRMarket.Validation.FirmValidators;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "HRMarket API", 
        Version = "v1",
        Description = "HRMarket API - Multi-language support via Accept-Language header"
    });
    
    c.ExampleFilters();
    
    // Add Accept-Language header to all endpoints
    c.OperationFilter<AcceptLanguageHeaderParameter>();
    
    // Optional: Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Translation and Language Services (BEFORE Identity)
builder.Services.AddScoped<ILanguageContext, LanguageContext>();
builder.Services.AddScoped<ITranslationService, TranslationService>();

// Identity Configuration with Custom Error Describer
builder.Services.AddIdentity<User, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddErrorDescriber<CustomIdentityErrorDescriber>()
.AddDefaultTokenProviders();

var sharedConfigPath = Path.Combine(builder.Environment.ContentRootPath, "..", "Common", "CommonSettings.json");
builder.Configuration.AddJsonFile(sharedConfigPath, optional: false, reloadOnChange: true);

// Token Services
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection(TokenSettings.SectionName));
builder.Services.AddSingleton<TokenSettings>(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TokenSettings>>().Value);

// Answer Services
builder.Services.AddScoped<IAnswerService, AnswerService>();

// Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Category Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();

// Firm Services
builder.Services.AddScoped<IFirmService, FirmService>();
builder.Services.AddScoped<IFirmRepository, FirmRepository>();

// Media Services
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IFileUploadBuilderFactory, FileUploadBuilderFactory>();
builder.Services.AddScoped<IMediaStorageRepo, S3MediaStorageRepo>();
builder.Services.AddScoped<AwsConfigurator>();
builder.Services.AddScoped<IMediaProducer, MediaProducer>();

// AWS S3 Configuration
builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection(AwsSettings.Section));
var awsSettings = builder.Configuration.GetSection(AwsSettings.Section).Get<AwsSettings>()
                  ?? throw new Exception("AWS settings are not configured properly.");
builder.Services.AddSingleton(awsSettings);

builder.Services.AddScoped<IAmazonS3>(sp =>
{
    var credentials = new Amazon.Runtime.BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);
    var region = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region);
    return new AmazonS3Client(credentials, region);
});

// Question Services
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Email Settings Configuration
builder.Services.Configure<EmailQueueSettings>(builder.Configuration.GetSection(EmailQueueSettings.SectionName));
builder.Services.AddSingleton<EmailQueueSettings>(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailQueueSettings>>().Value);

// Email Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<EmailProducer>();

// Entity Framework Validator
builder.Services.AddScoped<EntityValidator>();
builder.Services.AddScoped<CheckConstraintsDb>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFirmDtoValidator>();

// Exception Handler
builder.Services.AddExceptionHandling();

TypeAdapterConfig.GlobalSettings.Default
    .PreserveReference(true)
    .ShallowCopyForSameType(true)
    .EnableNonPublicMembers(true)
    .IgnoreNullValues(true)
    .MapToConstructor(true)
    .Unflattening(true);

FirmMapperConfig.Configure();

// JWT Configuration
var tokenSettings = builder.Configuration.GetSection(TokenSettings.SectionName).Get<TokenSettings>()
                   ?? throw new Exception("Token settings are not configured properly.");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var emailSettings = builder.Configuration.GetSection(EmailQueueSettings.SectionName).Get<EmailQueueSettings>();
        if (emailSettings == null) throw new InvalidOperationException("EmailSettings section is missing in configuration.");
        
        cfg.Host(emailSettings.Host, h =>
        {
            h.Username(emailSettings.Username);
            h.Password(emailSettings.Password);
        });

        cfg.Message<EmailMessage>(e => e.SetEntityName("email_queue"));
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = tokenSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = tokenSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRMarket API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// Language extraction BEFORE authentication
app.UseLanguageExtraction();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();
app.UseExceptionHandling();

app.Run();
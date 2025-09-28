using System.Text;
using Common.Email;
using Common.Media;
using HRMarket.Core.Answers;
using HRMarket.Core.Auth;
using HRMarket.Core.Auth.Tokens;
using HRMarket.Core.Categories;
using HRMarket.Core.Firms;
using HRMarket.Core.Media;
using HRMarket.Core.Questions;
using HRMarket.Entities;
using HRMarket.Entities.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Swagger UI 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRMarket API", Version = "v1" });
    c.ExampleFilters();
});

// Register Swagger example filters
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
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
.AddDefaultTokenProviders();

// Token Services
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection(TokenSettings.SectionName));
builder.Services.AddSingleton<TokenSettings>(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TokenSettings>>().Value);

// Answer Services
builder.Services.AddScoped<IAnswerService, AnswerService>();

//Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();

//Category Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();

// Firm Services
builder.Services.AddScoped<IFirmService, FirmService>();
builder.Services.AddScoped<IFirmRepository, FirmRepository>();

// Media Services
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IFileUploadBuilderFactory, FileUploadBuilderFactory>();
builder.Services.AddScoped<IMediaStorageRepo, S3MediaStorageRepo>();
// AWS S3 Configuration

// Question Services
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Email Settings Configuration
builder.Services.Configure<EmailQueueSettings>(
    builder.Configuration.GetSection(EmailQueueSettings.SectionName));
builder.Services.AddSingleton<EmailQueueSettings>(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailQueueSettings>>().Value);

// Authentication and Authorization - JWT Configuration
var tokenSettings = builder.Configuration.GetSection(TokenSettings.SectionName)
                       .Get<TokenSettings>()
                   ?? throw new Exception("Token settings are not configured properly.");

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
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(tokenSettings.SecretKey))
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
        }
    );

builder.Services.AddAuthorization();

// Business Services
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger in all environments for testing (you can restrict this later)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRMarket API v1");
    c.RoutePrefix = string.Empty; // This makes Swagger UI the default page
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
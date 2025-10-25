using HRMarket.Entities.Auth;
using HRMarket.Entities.Categories;
using HRMarket.Entities.Categories.Translations;
using HRMarket.Entities.LocationElements;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Firms.Firm> Firms { get; set; }
    public DbSet<Firms.FirmContact> FirmContacts { get; set; }
    public DbSet<Firms.FirmLinks> FirmLinks { get; set; }
    public DbSet<Firms.FirmLocation> FirmLocations { get; set; }
    public DbSet<Firms.FormForCategory> FormSubmissions { get; set; }
    public DbSet<Medias.Media> Medias { get; set; }
    public DbSet<Medias.FirmMedia> FirmMedias { get; set; }
    public DbSet<Questions.Question> Questions { get; set; }
    public DbSet<Questions.QuestionOption> QuestionOptions { get; set; }
    public DbSet<Questions.QuestionTranslation> QuestionTranslations { get; set; }
    public DbSet<Questions.OptionTranslation> OptionTranslations { get; set; }
    public DbSet<Answers.Answer> Answers { get; set; }
    public DbSet<Answers.AnswerOption> AnswerOptions { get; set; }
    public DbSet<Answers.AnswerTranslation> AnswerTranslations { get; set; }
    
    public DbSet<Categories.Category> Categories { get; set; }
    public DbSet<Categories.Cluster> Clusters { get; set; }
    public DbSet<Categories.Service> Services { get; set; }
    public DbSet<CategoryTranslation> CategoryTranslations { get; set; }
    public DbSet<ClusterTranslation> ClusterTranslations { get; set; }
    public DbSet<ServiceTranslation> ServiceTranslations { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<County> Counties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasPostgresExtension("uuid-ossp");        

        // Configure User entity
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        
        // Apply configurations for firm entities
        modelBuilder.ApplyConfiguration(new Firms.FirmConfiguration());
        modelBuilder.ApplyConfiguration(new Firms.FirmContactConfiguration());
        modelBuilder.ApplyConfiguration(new Firms.FirmLinksConfiguration());
        modelBuilder.ApplyConfiguration(new Firms.FirmLocationConfiguration());
        modelBuilder.ApplyConfiguration(new Firms.FormSubmissionConfiguration());
        
        // Apply configurations for media entities
        modelBuilder.ApplyConfiguration(new Medias.MediaConfiguration());
        modelBuilder.ApplyConfiguration(new Medias.FirmMediaConfiguration());
        
        // Apply configurations for question and answer entities
        modelBuilder.ApplyConfiguration(new Questions.QuestionConfiguration());
        modelBuilder.ApplyConfiguration(new Questions.OptionTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new Questions.QuestionTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new Questions.QuestionOptionConfiguration());
        
        modelBuilder.ApplyConfiguration(new Answers.AnswerConfiguration());
        modelBuilder.ApplyConfiguration(new Answers.AnswerOptionConfiguration());
        modelBuilder.ApplyConfiguration(new Answers.AnswerTranslationConfiguration());
        // Apply configurations for category entities
        modelBuilder.ApplyConfiguration(new Categories.CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new Categories.ClusterConfiguration()); 
        modelBuilder.ApplyConfiguration(new Categories.ServiceConfiguration());
        
        modelBuilder.ApplyConfiguration(new CategoryTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ClusterTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceTranslationConfiguration());
        
        // Apply configurations for location entities
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new CountyConfiguration());

    }
}
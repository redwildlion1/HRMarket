using HRMarket.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Questions;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(q => q.Type).IsRequired();
        builder.Property(q => q.Order).IsRequired();
        builder.Property(q => q.IsRequired).HasDefaultValue(false);
        builder.Property(q => q.IsFilter).HasDefaultValue(false);
        builder.Property(q => q.IsActive).HasDefaultValue(true);
        
        builder.Property(q => q.ValidationSchema)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.HasMany(q => q.Translations)
            .WithOne(t => t.Question)
            .HasForeignKey(t => t.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(q => q.Category)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(q => new { q.CategoryId, q.Order });
        builder.HasIndex(q => q.IsActive);
    }
}

public class QuestionTranslationConfiguration : IEntityTypeConfiguration<QuestionTranslation>
{
    public void Configure(EntityTypeBuilder<QuestionTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxQuestionTitleLength);
        
        builder.Property(t => t.Description)
            .HasMaxLength(AppConstants.MaxDescriptionLength);
        
        builder.Property(t => t.Placeholder)
            .HasMaxLength(200);
        
        builder.HasOne(t => t.Question)
            .WithMany(q => q.Translations)
            .HasForeignKey(t => t.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(t => new { t.QuestionId, t.LanguageCode }).IsUnique();
    }
}

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(o => o.Value)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(o => o.Order).IsRequired();
        builder.Property(o => o.IsActive).HasDefaultValue(true);
        
        builder.Property(o => o.Metadata).HasColumnType("jsonb");
        
        builder.HasMany(o => o.Translations)
            .WithOne(t => t.Option)
            .HasForeignKey(t => t.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(o => new { o.QuestionId, o.Order });
        builder.HasIndex(o => new { o.QuestionId, o.Value });
    }
}

public class OptionTranslationConfiguration : IEntityTypeConfiguration<OptionTranslation>
{
    public void Configure(EntityTypeBuilder<OptionTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(t => t.Label)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxOptionTextLength);
        
        builder.Property(t => t.Description)
            .HasMaxLength(300);
        
        builder.HasOne(t => t.Option)
            .WithMany(o => o.Translations)
            .HasForeignKey(t => t.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(t => new { t.OptionId, t.LanguageCode }).IsUnique();
    }
}
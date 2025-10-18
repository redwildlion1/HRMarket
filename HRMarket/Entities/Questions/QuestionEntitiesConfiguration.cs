using HRMarket.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Entities.Questions;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedOnAdd();

        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxQuestionTitleLength);

        builder.Property(q => q.Type)
            .IsRequired();

        builder.Property(q => q.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(q => q.Order)
            .IsRequired();

        builder.Property(q => q.IsFilter)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(q => q.ValidationJson)
            .HasColumnType("jsonb");
        

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Category)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Option> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.Value)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(o => o.Label)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxOptionTextLength);

        builder.Property(o => o.Order)
            .IsRequired();
        
        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionVariantConfiguration : IEntityTypeConfiguration<QuestionVariant>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<QuestionVariant> builder)
    {
        builder.HasKey(qv => qv.Id);
        builder.Property(qv => qv.Id).ValueGeneratedOnAdd();

        builder.Property(qv => qv.Value)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxQuestionTitleLength);

        builder.HasOne(qv => qv.Question)
            .WithMany(q => q.Variants)
            .HasForeignKey(qv => qv.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(qv => qv.Language)
            .WithMany()
            .HasForeignKey(qv => qv.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OptionVariantConfiguration : IEntityTypeConfiguration<OptionVariant>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OptionVariant> builder)
    {
        builder.HasKey(ov => ov.Id);
        builder.Property(ov => ov.Id).ValueGeneratedOnAdd();

        builder.Property(ov => ov.Value)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxOptionTextLength);

        builder.HasOne(ov => ov.Option)
            .WithMany(o => o.Variants)
            .HasForeignKey(ov => ov.OptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ov => ov.Language)
            .WithMany()
            .HasForeignKey(ov => ov.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
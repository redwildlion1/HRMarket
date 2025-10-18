using HRMarket.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Answers;

public class AnswersConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(a => a.Question)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(a => a.QuestionId)
            .IsRequired();
        
        builder.Property(a => a.NeedsUpdating)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(a => a.Value)
            .HasMaxLength(255);
        
        builder.Property(a => a.FormSubmissionId)
            .IsRequired();
        
        builder.HasOne(a => a.FormSubmission)
            .WithMany(fs => fs.Answers)
            .HasForeignKey(a => a.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(a => a.SelectedOptions)
            .WithMany(o => o.Answers)
            .UsingEntity<Dictionary<string, object>>(
                "AnswerOption",
                j => j
                    .HasOne<Option>()
                    .WithMany()
                    .HasForeignKey("OptionId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Answer>()
                    .WithMany()
                    .HasForeignKey("AnswerId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("AnswerId", "OptionId");
                    j.ToTable("AnswerOptions");
                });
    }
}

public class AnswerVariantConfiguration : IEntityTypeConfiguration<AnswerVariant>
{
    public void Configure(EntityTypeBuilder<AnswerVariant> builder)
    {
        builder.HasKey(av => av.Id);
        builder.Property(av => av.Id).HasDefaultValueSql("uuid_generate_v4())");
        
        builder.Property(av => av.Value)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(av => av.AnswerId)
            .IsRequired();
        
        builder.Property(av => av.LanguageId) 
            .IsRequired()
            .HasMaxLength(10);
        
        builder.HasOne(av => av.Answer)
            .WithMany(a => a.Variants)
            .HasForeignKey(av => av.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
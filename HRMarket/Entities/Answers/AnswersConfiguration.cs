using HRMarket.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Answers;

    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(a => a.Value).HasMaxLength(AppConstants.MaxAnswerValueLength);
            builder.Property(a => a.NeedsUpdating).HasDefaultValue(false);
            builder.Property(a => a.StructuredData).HasColumnType("jsonb");

            builder.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // configure composite FK to match FormForCategory PK (FirmId, CategoryId)
            builder.HasOne(a => a.FormForCategory)
                .WithMany(fs => fs.Answers)
                .HasForeignKey(a => new { a.FirmId, a.CategoryId })
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Translations)
                .WithOne(t => t.Answer)
                .HasForeignKey(t => t.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // unique constraint per (FirmId, CategoryId, QuestionId)
            builder.HasIndex(a => new { a.FirmId, a.CategoryId, a.QuestionId }).IsUnique();
            builder.HasIndex(a => a.NeedsUpdating);
        }
}

public class AnswerOptionConfiguration : IEntityTypeConfiguration<AnswerOption>
{
    public void Configure(EntityTypeBuilder<AnswerOption> builder)
    {
        builder.HasKey(ao => new { ao.AnswerId, ao.OptionId });

        builder.HasOne(ao => ao.Answer)
            .WithMany(a => a.SelectedOptions)
            .HasForeignKey(ao => ao.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ao => ao.Option)
            .WithMany()
            .HasForeignKey(ao => ao.OptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ao => ao.SelectedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}

public class AnswerTranslationConfiguration : IEntityTypeConfiguration<AnswerTranslation>
{
    public void Configure(EntityTypeBuilder<AnswerTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Value)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxAnswerValueLength);

        builder.HasOne(t => t.Answer)
            .WithMany(a => a.Translations)
            .HasForeignKey(t => t.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.AnswerId, t.LanguageCode }).IsUnique();
    }
}
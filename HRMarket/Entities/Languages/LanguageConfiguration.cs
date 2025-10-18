using Microsoft.EntityFrameworkCore;

namespace HRMarket.Entities.Languages;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
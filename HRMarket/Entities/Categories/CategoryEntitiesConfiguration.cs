using HRMarket.Configuration;
using HRMarket.Entities.Categories.Translations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Categories;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxIconLength);

        builder.Property(c => c.OrderInCluster)
            .IsRequired();

        builder.HasOne(c => c.Cluster)
            .WithMany(cl => cl.Categories)
            .HasForeignKey(c => c.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Translations)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Services)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Questions)
            .WithOne(q => q.Category)
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCategoryNameLength);

        builder.Property(t => t.Description)
            .HasMaxLength(AppConstants.MaxCategoryDescriptionLength);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.CategoryId, t.LanguageCode }).IsUnique();
    }
}

public class ClusterConfiguration : IEntityTypeConfiguration<Cluster>
{
    public void Configure(EntityTypeBuilder<Cluster> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxIconLength);

        builder.Property(c => c.OrderInList)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(c => c.Translations)
            .WithOne(t => t.Cluster)
            .HasForeignKey(t => t.ClusterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Categories)
            .WithOne(cat => cat.Cluster)
            .HasForeignKey(cat => cat.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ClusterTranslationConfiguration : IEntityTypeConfiguration<ClusterTranslation>
{
    public void Configure(EntityTypeBuilder<ClusterTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCategoryNameLength);

        builder.Property(t => t.Description)
            .HasMaxLength(AppConstants.MaxCategoryDescriptionLength);

        builder.HasOne(t => t.Cluster)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.ClusterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.ClusterId, t.LanguageCode }).IsUnique();
    }
}

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(s => s.OrderInCategory)
            .IsRequired();

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Translations)
            .WithOne(t => t.Service)
            .HasForeignKey(t => t.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ServiceTranslationConfiguration : IEntityTypeConfiguration<ServiceTranslation>
{
    public void Configure(EntityTypeBuilder<ServiceTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCategoryNameLength);

        builder.Property(t => t.Description)
            .HasMaxLength(AppConstants.MaxCategoryDescriptionLength);

        builder.HasOne(t => t.Service)
            .WithMany(s => s.Translations)
            .HasForeignKey(t => t.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.ServiceId, t.LanguageCode }).IsUnique();
    }
}
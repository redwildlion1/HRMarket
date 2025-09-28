using HRMarket.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Categories;

public class CategoryConfiguration :IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCategoryNameLength);

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxIconLength);

        builder.Property(c => c.OrderInCluster)
            .IsRequired();

        builder.HasOne(c => c.Cluster)
            .WithMany(cl => cl.Categories)
            .HasForeignKey(c => c.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);

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

public class ClusterConfiguration : IEntityTypeConfiguration<Cluster>
{
    public void Configure(EntityTypeBuilder<Cluster> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCategoryNameLength);

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxIconLength);

        builder.Property(c => c.OrderInList)
            .IsRequired();

        builder.HasMany(c => c.Categories)
            .WithOne(cat => cat.Cluster)
            .HasForeignKey(cat => cat.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCategoryNameLength);
        

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
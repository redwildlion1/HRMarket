using HRMarket.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.OuterAPIs.StripeApi;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxNameLength);
        
        builder.Property(sp => sp.Description)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxDescriptionLength);
        
        builder.Property(sp => sp.PriceMonthly)
            .HasPrecision(10, 2);
        
        builder.Property(sp => sp.PriceYearly)
            .HasPrecision(10, 2);
        
        builder.Property(sp => sp.StripePriceIdMonthly)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(sp => sp.StripePriceIdYearly)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(sp => sp.StripeProductId)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(sp => sp.Features)
            .HasColumnType("jsonb");
        
        builder.HasIndex(sp => sp.StripeProductId);
    }
}

public class FirmSubscriptionConfiguration : IEntityTypeConfiguration<FirmSubscription>
{
    public void Configure(EntityTypeBuilder<FirmSubscription> builder)
    {
        builder.HasKey(fs => fs.Id);
        builder.Property(fs => fs.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(fs => fs.StripeSubscriptionId)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(fs => fs.StripeCustomerId)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.HasOne(fs => fs.Firm)
            .WithMany()
            .HasForeignKey(fs => fs.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(fs => fs.SubscriptionPlan)
            .WithMany(sp => sp.FirmSubscriptions)
            .HasForeignKey(fs => fs.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(fs => fs.StripeSubscriptionId);
        builder.HasIndex(fs => fs.StripeCustomerId);
        builder.HasIndex(fs => new { fs.FirmId, fs.Status });
    }
}

public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure(EntityTypeBuilder<PaymentHistory> builder)
    {
        builder.HasKey(ph => ph.Id);
        builder.Property(ph => ph.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(ph => ph.StripeInvoiceId)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(ph => ph.StripePaymentIntentId)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxTokenLength);
        
        builder.Property(ph => ph.Amount)
            .HasPrecision(10, 2);
        
        builder.Property(ph => ph.Currency)
            .IsRequired()
            .HasMaxLength(3);
        
        builder.HasOne(ph => ph.Firm)
            .WithMany()
            .HasForeignKey(ph => ph.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(ph => ph.StripeInvoiceId);
        builder.HasIndex(ph => new { ph.FirmId, ph.CreatedAt });
    }
}
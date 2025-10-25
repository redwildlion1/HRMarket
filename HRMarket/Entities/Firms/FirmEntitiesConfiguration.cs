using HRMarket.Configuration;
using HRMarket.Configuration.Status;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Firms;

// Firm configuration
public class FirmConfiguration : IEntityTypeConfiguration<Firm>
{
    public void Configure(EntityTypeBuilder<Firm> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasDefaultValueSql("uuid_generate_v4()");

        builder.HasOne(f => f.Contact)
            .WithOne()
            .HasForeignKey<FirmContact>(c => c.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Links)
            .WithOne()
            .HasForeignKey<FirmLinks>(l => l.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Location)
            .WithOne()
            .HasForeignKey<FirmLocation>(l => l.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Forms)
            .WithOne(fs => fs.Firm)
            .HasForeignKey(fs => fs.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.Cui)
            .IsRequired()
            .HasMaxLength(AppConstants.CuiLength);
        
        builder.HasIndex(f => f.Cui)
            .IsUnique();
        
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxCompanyNameLength);
        
        builder.Property(f => f.Type)
            .IsRequired();
           
        builder.Property(f => f.Description)
            .HasMaxLength(AppConstants.MaxDescriptionLength);
        
        builder.Property(f => f.Status)
            .IsRequired()
            .HasDefaultValue(FirmStatus.Draft);
        
        builder.Property(f => f.SubmittedForReviewAt);
        
        builder.Property(f => f.ReviewedAt);
        
        builder.Property(f => f.ReviewedByUserId);
        
        builder.Property(f => f.RejectionReason)
            .HasMaxLength(1000);
        
        builder.Property(f => f.RejectionReasonType);
        
        builder.HasIndex(f => f.Status);
    }
}

// FirmContact configuration
public class FirmContactConfiguration : IEntityTypeConfiguration<FirmContact>
{
    public void Configure(EntityTypeBuilder<FirmContact> builder)
    {
        builder.HasKey(c => c.FirmId);
        builder.Property(c => c.Email).HasMaxLength(AppConstants.MaxEmailLength);
        builder.Property(c => c.Phone).HasMaxLength(AppConstants.MaxPhoneLength);
        
        builder.HasOne(c => c.Firm)
            .WithOne(f => f.Contact)
            .HasForeignKey<FirmContact>(c => c.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// FirmLinks configuration
public class FirmLinksConfiguration : IEntityTypeConfiguration<FirmLinks>
{
    public void Configure(EntityTypeBuilder<FirmLinks> builder)
    {
        builder.HasKey(l => l.FirmId);
        builder.Property(l => l.Website).HasMaxLength(AppConstants.MaxWebsiteLength);
        builder.Property(l => l.LinkedIn).HasMaxLength(AppConstants.MaxWebsiteLength);
        builder.Property(l => l.Facebook).HasMaxLength(AppConstants.MaxWebsiteLength);
        builder.Property(l => l.Twitter).HasMaxLength(AppConstants.MaxWebsiteLength);
        builder.Property(l => l.Instagram).HasMaxLength(AppConstants.MaxWebsiteLength);
        
        builder.HasOne(l => l.Firm)
            .WithOne(f => f.Links)
            .HasForeignKey<FirmLinks>(l => l.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// FirmLocation configuration
public class FirmLocationConfiguration : IEntityTypeConfiguration<FirmLocation>
{
    public void Configure(EntityTypeBuilder<FirmLocation> builder)
    {
        builder.HasKey(l => l.FirmId);
        builder.Property(l => l.Address).HasMaxLength(AppConstants.MaxAddressLength);
        builder.Property(l => l.PostalCode).HasMaxLength(AppConstants.MaxPostalCodeLength);
        
        builder.Property(l => l.City).HasMaxLength(AppConstants.MaxLocationNameLength);
        
        builder.HasOne(l => l.Country)
            .WithMany()
            .HasForeignKey(l => l.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(l => l.County)
            .WithMany()
            .HasForeignKey(l => l.CountyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        builder.HasIndex(l => new { l.CountryId, l.CountyId });
        
        builder.HasOne(l => l.Firm)
            .WithOne(f => f.Location)
            .HasForeignKey<FirmLocation>(l => l.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// FormSubmission configuration
public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormForCategory>
{
    public void Configure(EntityTypeBuilder<FormForCategory> builder)
    {
        builder.HasKey(fs => new { fs.FirmId, fs.CategoryId });
        builder.HasMany(fs => fs.Answers)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(fs => fs.Firm)
            .WithMany(f => f.Forms)
            .HasForeignKey(fs => fs.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(fs => fs.Category)
            .WithMany()
            .HasForeignKey(fs => fs.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(fs => fs.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(fs => fs.Message).HasMaxLength(AppConstants.MaxFormMessageLength);
    }
}

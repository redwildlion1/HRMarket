using HRMarket.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Medias;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(m => m.OriginalFileName)
            .IsRequired()
            .HasMaxLength(AppConstants.MaxFileNameLength);
        builder.Property(m => m.FileType)
            .IsRequired();
        builder.Property(m => m.SizeInBytes)
            .IsRequired();
        builder.Property(m => m.S3KeyTemp)
            .HasMaxLength(AppConstants.MaxS3KeyLength);
        builder.Property(m => m.S3KeyFinal)
            .HasMaxLength(AppConstants.MaxS3KeyLength);
        builder.Property(m => m.Width);
        builder.Property(m => m.Height);
        builder.Property(m => m.Status)
            .IsRequired();
        builder.Property(m => m.CreatedAt)
            .IsRequired();  
        
        builder.HasOne(m => m.FirmMedia)
            .WithOne(fm => fm.Media)
            .HasForeignKey<FirmMedia>(fm => fm.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FirmMediaConfiguration : IEntityTypeConfiguration<FirmMedia>
{
    public void Configure(EntityTypeBuilder<FirmMedia> builder)
    {
        builder.HasKey(fm => fm.Id);
        builder.Property(fm => fm.Id).HasDefaultValueSql("uuid_generate_v4()");
        
        builder.Property(fm => fm.FirmMediaType)
            .IsRequired();
        
        builder.HasOne(fm => fm.Firm)
            .WithMany(f => f.FirmMedias)
            .HasForeignKey(fm => fm.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(fm => fm.Media)
            .WithOne(m => m.FirmMedia)
            .HasForeignKey<FirmMedia>(fm => fm.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
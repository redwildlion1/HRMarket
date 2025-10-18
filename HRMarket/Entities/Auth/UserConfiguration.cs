using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMarket.Entities.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed admin user
        var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var adminUser = new User
        {
            Id = adminUserId,
            UserName = "admin@hrmarket.com",
            NormalizedUserName = "ADMIN@HRMARKET.COM",
            Email = "admin@hrmarket.com",
            NormalizedEmail = "ADMIN@HRMARKET.COM",
            EmailConfirmed = true,
            Newsletter = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0
        };

        // Hash password: "Admin@123" (change this to your desired password)
        var passwordHasher = new PasswordHasher<User>();
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

        builder.HasData(adminUser);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasData(
            new Role 
            { 
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                Name = "Admin", 
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new Role 
            { 
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                Name = "User", 
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
            );
    }
}
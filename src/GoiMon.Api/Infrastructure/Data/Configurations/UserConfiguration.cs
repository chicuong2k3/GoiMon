using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // Unique constraints
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.GoogleId).IsUnique();
        builder.HasIndex(u => u.FacebookId).IsUnique();

        // Required properties
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);

        // Optional properties
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.GoogleId).HasMaxLength(500);
        builder.Property(u => u.FacebookId).HasMaxLength(500);
        builder.Property(u => u.PhotoUrl).HasMaxLength(500);

        // Timestamps
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();

        // Boolean defaults
        builder.Property(u => u.IsVerified).HasDefaultValue(false);
        builder.Property(u => u.IsActive).HasDefaultValue(true);

        // Relationships
        builder.HasMany(u => u.OtpTokens)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

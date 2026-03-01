using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class OtpTokenConfiguration : IEntityTypeConfiguration<OtpToken>
{
    public void Configure(EntityTypeBuilder<OtpToken> builder)
    {
        builder.HasKey(o => o.Id);

        // Indexes
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => new { o.UserId, o.IsUsed, o.ExpiresAt });

        // Required properties
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Token).IsRequired().HasMaxLength(10);
        builder.Property(o => o.DeliveryMethod).IsRequired();
            // .HasConversion<string>() // Optional: Store as string in DB, uncomment if needed

        // Timestamps
        builder.Property(o => o.ExpiresAt).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UsedAt).IsRequired(false);

        // Boolean and numeric defaults
        builder.Property(o => o.IsUsed).HasDefaultValue(false);
        builder.Property(o => o.FailedAttempts).HasDefaultValue(0);

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany(u => u.OtpTokens)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.CreatedAt).IsRequired();
        // Store enum as string in the database
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

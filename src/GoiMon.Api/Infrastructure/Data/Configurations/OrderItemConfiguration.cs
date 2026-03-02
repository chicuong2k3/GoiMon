using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.OrderId).IsRequired();

        // Soft reference — nullable because a product can be deleted after ordering
        builder.Property(oi => oi.ProductId).IsRequired(false);

        // Immutable snapshots captured at order-time
        builder.Property(oi => oi.ProductName).IsRequired().HasMaxLength(255);
        builder.Property(oi => oi.UnitName).IsRequired(false).HasMaxLength(50);
        builder.Property(oi => oi.ComboId).IsRequired(false);
        builder.Property(oi => oi.ComboName).IsRequired(false).HasMaxLength(255);

        builder.Property(oi => oi.Qty).IsRequired();
        builder.Property(oi => oi.UnitPrice).IsRequired().HasPrecision(18, 2);
    }
}

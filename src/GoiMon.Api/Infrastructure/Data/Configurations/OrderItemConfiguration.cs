using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.OrderId).IsRequired();
        builder.Property(oi => oi.ProductId).IsRequired();
        builder.Property(oi => oi.Qty).IsRequired();
        builder.Property(oi => oi.UnitPriceCents).IsRequired();
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class OrderItemModifierConfiguration : IEntityTypeConfiguration<OrderItemModifier>
{
    public void Configure(EntityTypeBuilder<OrderItemModifier> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.OrderItemId).IsRequired();
        builder.Property(m => m.ModifierOptionId).IsRequired(false);
        builder.Property(m => m.GroupName).IsRequired().HasMaxLength(120);
        builder.Property(m => m.OptionName).IsRequired().HasMaxLength(120);
        builder.Property(m => m.Qty).IsRequired();
        builder.Property(m => m.UnitDeltaPrice).IsRequired().HasPrecision(18, 2);

        builder.HasOne<OrderItem>()
            .WithMany()
            .HasForeignKey(m => m.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.OrderItemId);
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class ModifierOptionConfiguration : IEntityTypeConfiguration<ModifierOption>
{
    public void Configure(EntityTypeBuilder<ModifierOption> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.ModifierGroupId).IsRequired();
        builder.Property(o => o.Name).IsRequired().HasMaxLength(120);
        builder.Property(o => o.PriceDelta).IsRequired().HasPrecision(18, 2);
        builder.Property(o => o.MaxQty).IsRequired();
        builder.Property(o => o.SortOrder).IsRequired();
        builder.Property(o => o.IsDefault).IsRequired();
        builder.Property(o => o.IsActive).IsRequired();

        builder.HasIndex(o => new { o.ModifierGroupId, o.SortOrder });
    }
}

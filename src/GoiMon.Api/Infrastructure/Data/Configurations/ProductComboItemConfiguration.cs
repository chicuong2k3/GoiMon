using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class ProductComboItemConfiguration : IEntityTypeConfiguration<ProductComboItem>
{
    public void Configure(EntityTypeBuilder<ProductComboItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ComboId).IsRequired();
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.VariantId).IsRequired(false);
        builder.Property(i => i.Qty).IsRequired();

        builder.HasIndex(i => i.VariantId);
    }
}

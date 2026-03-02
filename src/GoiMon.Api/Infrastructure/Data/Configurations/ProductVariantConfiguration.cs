using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProductId).IsRequired();
        builder.Property(v => v.Code).IsRequired().HasMaxLength(50);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(120);
        builder.Property(v => v.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(v => v.SortOrder).IsRequired();
        builder.Property(v => v.IsActive).IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.ProductId, v.Code }).IsUnique();
        builder.HasIndex(v => new { v.ProductId, v.SortOrder });
    }
}

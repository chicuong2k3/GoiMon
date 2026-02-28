using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class ProductComboConfiguration : IEntityTypeConfiguration<ProductCombo>
{
    public void Configure(EntityTypeBuilder<ProductCombo> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.Price).IsRequired().HasPrecision(18, 2);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.ComboId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

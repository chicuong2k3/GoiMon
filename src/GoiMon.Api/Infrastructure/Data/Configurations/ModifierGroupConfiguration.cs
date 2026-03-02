using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public class ModifierGroupConfiguration : IEntityTypeConfiguration<ModifierGroup>
{
    public void Configure(EntityTypeBuilder<ModifierGroup> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.ProductId).IsRequired();
        builder.Property(g => g.Name).IsRequired().HasMaxLength(120);
        builder.Property(g => g.SelectionMode)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(g => g.MinSelect).IsRequired();
        builder.Property(g => g.MaxSelect).IsRequired();
        builder.Property(g => g.SortOrder).IsRequired();
        builder.Property(g => g.IsRequired).IsRequired();
        builder.Property(g => g.IsActive).IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(g => g.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Options)
            .WithOne()
            .HasForeignKey(o => o.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => new { g.ProductId, g.SortOrder });
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoiMon.Api.Infrastructure.Data.Configurations;

public sealed class TableSlotConfiguration : IEntityTypeConfiguration<TableSlot>
{
    public void Configure(EntityTypeBuilder<TableSlot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CurrentState)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(Domain.Enums.TableServiceState.Available);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

using GoiMon.Api.Domain;

namespace GoiMon.Api.Domain.Entities;

public class ProductVariant : IMultiTenant
{
    private ProductVariant() { }

    public ProductVariant(
        Guid id,
        Guid productId,
        string code,
        string name,
        decimal price,
        int sortOrder = 0,
        bool isActive = true,
        Guid tenantId = default)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Variant code cannot be empty", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Variant name cannot be empty", nameof(name));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

        Id = id;
        TenantId = tenantId;
        ProductId = productId;
        Code = code.Trim().ToLowerInvariant();
        Name = name.Trim();
        Price = price;
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    public Guid TenantId { get; set; }

    /// <summary>
    /// Stable identifier for this variant record.
    /// Needed for referencing the selected variant in order inputs and historical snapshots.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Parent product identifier.
    /// Needed to group all variants (S/M/L...) under the same sellable product.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Short machine-friendly code (e.g. s, m, l).
    /// Needed for stable integration and uniqueness checks beyond display text.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable variant label shown in UI (e.g. "Size M").
    /// Needed for cashier/customer clarity.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Final base price for this variant.
    /// Needed to support explicit per-size pricing instead of hard-coded deltas.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Display order among sibling variants.
    /// Needed to keep deterministic UI ordering (S -> M -> L).
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Soft availability flag.
    /// Needed to temporarily disable a variant without deleting historical references.
    /// </summary>
    public bool IsActive { get; private set; }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Variant name cannot be empty", nameof(name));
        Name = name.Trim();
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        Price = price;
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}

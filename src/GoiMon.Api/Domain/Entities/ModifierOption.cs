using GoiMon.Api.Domain;

namespace GoiMon.Api.Domain.Entities;

public class ModifierOption : IMultiTenant
{
    private ModifierOption() { }

    public ModifierOption(
        Guid id,
        Guid modifierGroupId,
        string name,
        decimal priceDelta,
        int maxQty = 1,
        int sortOrder = 0,
        bool isDefault = false,
        bool isActive = true,
        Guid tenantId = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Modifier option name cannot be empty", nameof(name));
        if (priceDelta < 0) throw new ArgumentOutOfRangeException(nameof(priceDelta));
        if (maxQty <= 0) throw new ArgumentOutOfRangeException(nameof(maxQty));

        Id = id;
        TenantId = tenantId;
        ModifierGroupId = modifierGroupId;
        Name = name.Trim();
        PriceDelta = priceDelta;
        MaxQty = maxQty;
        SortOrder = sortOrder;
        IsDefault = isDefault;
        IsActive = isActive;
    }

    public Guid TenantId { get; set; }

    /// <summary>
    /// Stable identifier for this option.
    /// Needed for selection payloads and historical references.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Parent modifier group identifier.
    /// Needed to validate that the option belongs to the selected group.
    /// </summary>
    public Guid ModifierGroupId { get; private set; }

    /// <summary>
    /// Option display label (e.g. "Pearl").
    /// Needed for UI and receipt readability.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Additional price per one option quantity unit.
    /// Needed for deterministic line price calculation.
    /// </summary>
    public decimal PriceDelta { get; private set; }

    /// <summary>
    /// Maximum quantity allowed for this option in one order line.
    /// Needed to enforce customization limits.
    /// </summary>
    public int MaxQty { get; private set; }

    /// <summary>
    /// Display order among sibling options.
    /// Needed to keep consistent UX ordering.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Marks option as preselected by default.
    /// Needed for quick-order presets and configurable defaults.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Soft availability flag for this option.
    /// Needed to temporarily disable an option without deleting history.
    /// </summary>
    public bool IsActive { get; private set; }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Modifier option name cannot be empty", nameof(name));
        Name = name.Trim();
    }

    public void UpdatePriceDelta(decimal priceDelta)
    {
        if (priceDelta < 0) throw new ArgumentOutOfRangeException(nameof(priceDelta));
        PriceDelta = priceDelta;
    }

    public void UpdateMaxQty(int maxQty)
    {
        if (maxQty <= 0) throw new ArgumentOutOfRangeException(nameof(maxQty));
        MaxQty = maxQty;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}

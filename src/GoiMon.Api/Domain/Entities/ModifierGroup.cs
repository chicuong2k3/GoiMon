using GoiMon.Api.Domain;

namespace GoiMon.Api.Domain.Entities;

public enum ModifierSelectionMode
{
    /// <summary>
    /// Exactly one option should be selected from the group.
    /// Used for choices like sugar level or milk type.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Multiple options can be selected in the group.
    /// Used for add-on scenarios like toppings.
    /// </summary>
    Multiple = 2
}

public class ModifierGroup : IMultiTenant
{
    private ModifierGroup() { Options = new List<ModifierOption>(); }

    public ModifierGroup(
        Guid id,
        Guid tenantId,
        Guid productId,
        string name,
        ModifierSelectionMode selectionMode,
        int minSelect,
        int maxSelect,
        int sortOrder = 0,
        bool isRequired = false,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Modifier group name cannot be empty", nameof(name));
        if (minSelect < 0) throw new ArgumentOutOfRangeException(nameof(minSelect));
        if (maxSelect < minSelect) throw new ArgumentOutOfRangeException(nameof(maxSelect));

        Id = id;
        TenantId = tenantId;
        ProductId = productId;
        Name = name.Trim();
        SelectionMode = selectionMode;
        MinSelect = minSelect;
        MaxSelect = maxSelect;
        SortOrder = sortOrder;
        IsRequired = isRequired;
        IsActive = isActive;
        Options = new List<ModifierOption>();
    }

    public Guid TenantId { get; set; }

    /// <summary>
    /// Stable identifier for this modifier group.
    /// Needed for query/mutation references and data migration safety.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Product that owns this group.
    /// Needed to scope options so different products can have different customization sets.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Group label shown in UI (e.g. "Topping", "Sugar").
    /// Needed for clear option organization during ordering.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Selection behavior (single or multiple).
    /// Needed to enforce business rules consistently across API and client.
    /// </summary>
    public ModifierSelectionMode SelectionMode { get; private set; }

    /// <summary>
    /// Minimum required selections in this group.
    /// Needed for validation of required choices.
    /// </summary>
    public int MinSelect { get; private set; }

    /// <summary>
    /// Maximum allowed selections in this group.
    /// Needed to prevent invalid/overloaded configurations.
    /// </summary>
    public int MaxSelect { get; private set; }

    /// <summary>
    /// Display order among groups.
    /// Needed for predictable rendering sequence in the configurator.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Indicates this group must be answered to submit order line.
    /// Needed to model mandatory business options.
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Soft availability flag for the whole group.
    /// Needed to temporarily hide a group without deleting historical data.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Options contained in this group.
    /// Needed to represent concrete selectable values for customization.
    /// </summary>
    public List<ModifierOption> Options { get; private set; }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Modifier group name cannot be empty", nameof(name));
        Name = name.Trim();
    }

    public void UpdateSelection(ModifierSelectionMode selectionMode, int minSelect, int maxSelect)
    {
        if (minSelect < 0) throw new ArgumentOutOfRangeException(nameof(minSelect));
        if (maxSelect < minSelect) throw new ArgumentOutOfRangeException(nameof(maxSelect));

        SelectionMode = selectionMode;
        MinSelect = minSelect;
        MaxSelect = maxSelect;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}

using GoiMon.Api.Domain;

namespace GoiMon.Api.Domain.Entities;

public class OrderItemModifier : IMultiTenant
{
    private OrderItemModifier() { }

    public OrderItemModifier(
        Guid id,
        Guid orderItemId,
        Guid? modifierOptionId,
        string groupName,
        string optionName,
        int qty,
        decimal unitDeltaPrice,
        Guid tenantId = default)
    {
        if (string.IsNullOrWhiteSpace(groupName)) throw new ArgumentException("Modifier group name is required", nameof(groupName));
        if (string.IsNullOrWhiteSpace(optionName)) throw new ArgumentException("Modifier option name is required", nameof(optionName));
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (unitDeltaPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitDeltaPrice));

        Id = id;
        TenantId = tenantId;
        OrderItemId = orderItemId;
        ModifierOptionId = modifierOptionId;
        GroupName = groupName.Trim();
        OptionName = optionName.Trim();
        Qty = qty;
        UnitDeltaPrice = unitDeltaPrice;
    }

    public Guid TenantId { get; set; }

    /// <summary>
    /// Stable identifier of the selected modifier snapshot row.
    /// Needed for auditing and future updates on order details.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Parent order item identifier.
    /// Needed to attach selected modifiers to the correct order line.
    /// </summary>
    public Guid OrderItemId { get; private set; }

    /// <summary>
    /// Optional reference to catalog option id.
    /// Needed for analytics while remaining safe if catalog option is removed later.
    /// </summary>
    public Guid? ModifierOptionId { get; private set; }

    /// <summary>
    /// Snapshot of group name at order time.
    /// Needed so receipts/history remain readable even if catalog names change.
    /// </summary>
    public string GroupName { get; private set; } = string.Empty;

    /// <summary>
    /// Snapshot of option name at order time.
    /// Needed for immutable historical display.
    /// </summary>
    public string OptionName { get; private set; } = string.Empty;

    /// <summary>
    /// Quantity of this option selected on the line.
    /// Needed for correct pricing and constraint validation.
    /// </summary>
    public int Qty { get; private set; }

    /// <summary>
    /// Snapshot of per-unit extra price for this option at order time.
    /// Needed to protect historical totals from later price changes.
    /// </summary>
    public decimal UnitDeltaPrice { get; private set; }
}

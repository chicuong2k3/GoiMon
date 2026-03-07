namespace GoiMon.Api.Domain.Entities;
using GoiMon.Api.Domain;

/// <summary>
/// Aggregate root representing a customer order and its line items.
/// </summary>
public class Order : AggregateRoot, IMultiTenant
{
    private Order() { Items = new List<OrderItem>(); }

    /// <summary>
    /// Create a new order with the specified identifier.
    /// </summary>
    /// <param name="id">Identifier for the order.</param>
    /// <param name="tenantId">Identifier for the tenant.</param>
    public Order(Guid id, Guid tenantId)
    {
        Id = id;
        TenantId = tenantId;
        Status = OrderStatus.Open;
        Items = new List<OrderItem>();
        Total = 0m;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new Events.OrderCreatedEvent(id));
    }

    public Guid TenantId { get; set; }

    /// <summary>UTC timestamp when the order was placed.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Current status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; } = OrderStatus.Open;

    /// <summary>
    /// Total amount for the order.
    /// </summary>
    public decimal Total { get; private set; }

    /// <summary>
    /// Optional table slot assignment. Null means takeaway/no-table.
    /// </summary>
    public Guid? TableSlotId { get; private set; }

    /// <summary>
    /// Mutable list used internally to store order items. Exposed as read-only in contracts.
    /// </summary>
    public List<OrderItem> Items { get; private set; }

    /// <summary>
    /// Add a new line item to the order with a point-in-time product snapshot.
    /// </summary>
    /// <param name="productId">Optional reference to the source product (for analytics/BI only).</param>
    /// <param name="productName">Snapshot of the product name at order time.</param>
    /// <param name="qty">Quantity to add; must be greater than zero.</param>
    /// <param name="unitPrice">Unit price at order time; must be non-negative.</param>
    /// <param name="unitName">Snapshot of the product's unit label at order time (e.g. "phần", "ly").</param>
    /// <param name="comboId">Optional soft-reference to source combo (for analytics/BI only).</param>
    /// <param name="comboName">Snapshot of the combo name at order time.</param>
    public void AddItem(
        Guid? productId,
        string productName,
        int qty,
        decimal unitPrice,
        string? unitName = null,
        Guid? comboId = null,
        string? comboName = null)
    {
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("Product name snapshot is required.", nameof(productName));
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

        var item = new OrderItem(Guid.NewGuid(), TenantId, Id, productId, productName, qty, unitPrice, unitName, comboId, comboName);
        Items.Add(item);
        RecalculateTotal();
        AddDomainEvent(new Events.OrderItemAddedEvent(Id, item.Id, productId, qty));
    }

    /// <summary>
    /// Remove an item from the order by its identifier.
    /// </summary>
    /// <param name="orderItemId">Identifier of the order item to remove.</param>
    public void RemoveItem(Guid orderItemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == orderItemId);
        if (item is null) return;
        Items.Remove(item);
        RecalculateTotal();
    }

    /// <summary>
    /// Mark the order as completed.
    /// </summary>
    public void MarkCompleted()
    {
        if (Status != OrderStatus.Open) throw new InvalidOperationException("Only open orders can be completed.");
        Status = OrderStatus.Completed;
        AddDomainEvent(new Events.OrderCompletedEvent(Id));
    }

    /// <summary>
    /// Mark the order as paid. Only completed orders can be paid.
    /// </summary>
    public void MarkPaid()
    {
        if (Status != OrderStatus.Completed) throw new InvalidOperationException("Only completed orders can be marked as paid.");
        Status = OrderStatus.Paid;
        AddDomainEvent(new Events.OrderPaidEvent(Id));
    }

    /// <summary>
    /// Cancel the order. Only open orders can be cancelled.
    /// </summary>
    public void Cancel()
    {
        if (Status != OrderStatus.Open) throw new InvalidOperationException("Only open orders can be cancelled.");
        Status = OrderStatus.Cancelled;
        AddDomainEvent(new Events.OrderCancelledEvent(Id));
    }

    public void AssignTableSlot(Guid? tableSlotId)
    {
        TableSlotId = tableSlotId;
    }

    private void RecalculateTotal()
    {
        Total = Items.Sum(i => i.Qty * i.UnitPrice);
    }
}

/// <summary>
/// A single line item belonging to an <see cref="Order"/>.
/// Stores an immutable snapshot of product data at the time of ordering so that
/// price/name changes or product deletion never corrupt historical records.
/// </summary>
public class OrderItem : IMultiTenant
{
    private OrderItem() { }

    /// <summary>
    /// Create a new order item instance with a product snapshot.
    /// </summary>
    public OrderItem(
        Guid id,
        Guid tenantId,
        Guid orderId,
        Guid? productId,
        string productName,
        int qty,
        decimal unitPrice,
        string? unitName = null,
        Guid? comboId = null,
        string? comboName = null)
    {
        Id = id;
        TenantId = tenantId;
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitName = unitName;
        ComboId = comboId;
        ComboName = comboName;
        Qty = qty;
        UnitPrice = unitPrice;
    }

    public Guid TenantId { get; set; }

    /// <summary>
    /// Identifier for the order item.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifier of the parent order.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// Optional soft-reference to the source product (for BI/analytics only).
    /// Do NOT use this for display or pricing — read <see cref="ProductName"/> and <see cref="UnitPrice"/> instead.
    /// </summary>
    public Guid? ProductId { get; private set; }

    /// <summary>
    /// Immutable snapshot of the product name at the time the order was placed.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>Snapshot of the product's unit label at order time (e.g. "phần", "ly").</summary>
    public string? UnitName { get; private set; }

    /// <summary>
    /// Optional soft-reference to source combo (for BI/analytics only).
    /// </summary>
    public Guid? ComboId { get; private set; }

    /// <summary>
    /// Immutable snapshot of combo name at order time.
    /// </summary>
    public string? ComboName { get; private set; }

    /// <summary>
    /// Quantity of the product for this line item.
    /// </summary>
    public int Qty { get; private set; }

    /// <summary>
    /// Unit price for the product at the time of ordering.
    /// </summary>
    public decimal UnitPrice { get; private set; }
}

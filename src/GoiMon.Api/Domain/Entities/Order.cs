namespace GoiMon.Api.Domain.Entities;
/// <summary>
/// Aggregate root representing a customer order and its line items.
/// </summary>
public class Order : AggregateRoot
{
    private Order() { Items = new List<OrderItem>(); }

    /// <summary>
    /// Create a new order with the specified identifier.
    /// </summary>
    /// <param name="id">Identifier for the order.</param>
    public Order(Guid id)
    {
        Id = id;
        Status = "open";
        Items = new List<OrderItem>();
        TotalCents = 0;
    }

    /// <summary>
    /// Current status of the order (for example "open" or "completed").
    /// </summary>
    public string Status { get; private set; } = "open";

    /// <summary>
    /// Total amount for the order in cents.
    /// </summary>
    public int TotalCents { get; private set; }

    /// <summary>
    /// Mutable list used internally to store order items. Exposed as read-only in contracts.
    /// </summary>
    public List<OrderItem> Items { get; private set; }

    /// <summary>
    /// Add a new line item to the order and recalculate the total.
    /// </summary>
    /// <param name="productId">Referenced product identifier.</param>
    /// <param name="qty">Quantity to add; must be greater than zero.</param>
    /// <param name="unitPriceCents">Unit price in cents; must be non-negative.</param>
    public void AddItem(Guid productId, int qty, int unitPriceCents)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (unitPriceCents < 0) throw new ArgumentOutOfRangeException(nameof(unitPriceCents));

        var item = new OrderItem(Guid.NewGuid(), Id, productId, qty, unitPriceCents);
        Items.Add(item);
        RecalculateTotal();
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
        Status = "completed";
    }

    private void RecalculateTotal()
    {
        TotalCents = Items.Sum(i => i.Qty * i.UnitPriceCents);
    }
}

/// <summary>
/// A single line item belonging to an <see cref="Order"/>.
/// </summary>
public class OrderItem
{
    private OrderItem() { }

    /// <summary>
    /// Create a new order item instance.
    /// </summary>
    public OrderItem(Guid id, Guid orderId, Guid productId, int qty, int unitPriceCents)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Qty = qty;
        UnitPriceCents = unitPriceCents;
    }

    /// <summary>
    /// Identifier for the order item.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifier of the parent order.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// Identifier of the referenced product.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Quantity of the product for this line item.
    /// </summary>
    public int Qty { get; private set; }

    /// <summary>
    /// Unit price in cents for the product at the time of ordering.
    /// </summary>
    public int UnitPriceCents { get; private set; }
}

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
        Status = OrderStatus.Open;
        Items = new List<OrderItem>();
        Total = 0m;
        AddDomainEvent(new Events.OrderCreatedEvent(id));
    }

    /// <summary>
    /// Current status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; } = OrderStatus.Open;

    /// <summary>
    /// Total amount for the order.
    /// </summary>
    public decimal Total { get; private set; }

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
    public void AddItem(Guid productId, int qty, decimal unitPrice)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

        var item = new OrderItem(Guid.NewGuid(), Id, productId, qty, unitPrice);
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
        Status = OrderStatus.Completed;
        AddDomainEvent(new Events.OrderCompletedEvent(Id));
    }

    private void RecalculateTotal()
    {
        Total = Items.Sum(i => i.Qty * i.UnitPrice);
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
    public OrderItem(Guid id, Guid orderId, Guid productId, int qty, decimal unitPrice)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Qty = qty;
        UnitPrice = unitPrice;
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
    /// Unit price for the product at the time of ordering.
    /// </summary>
    public decimal UnitPrice { get; private set; }
}

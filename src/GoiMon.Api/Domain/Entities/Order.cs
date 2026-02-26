namespace GoiMon.Api.Domain.Entities;

using GoiMon.Api.Domain;

public class Order : AggregateRoot
{
    private Order() { Items = new List<OrderItem>(); }

    public Order(Guid id)
    {
        Id = id;
        Status = "open";
        Items = new List<OrderItem>();
        TotalCents = 0;
    }

    public string Status { get; private set; } = "open";
    public int TotalCents { get; private set; }
    public List<OrderItem> Items { get; private set; }

    public void AddItem(Guid productId, int qty, int unitPriceCents)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (unitPriceCents < 0) throw new ArgumentOutOfRangeException(nameof(unitPriceCents));

        var item = new OrderItem(Guid.NewGuid(), Id, productId, qty, unitPriceCents);
        Items.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(Guid orderItemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == orderItemId);
        if (item is null) return;
        Items.Remove(item);
        RecalculateTotal();
    }

    public void MarkCompleted()
    {
        Status = "completed";
    }

    private void RecalculateTotal()
    {
        TotalCents = Items.Sum(i => i.Qty * i.UnitPriceCents);
    }
}

public class OrderItem
{
    private OrderItem() { }

    public OrderItem(Guid id, Guid orderId, Guid productId, int qty, int unitPriceCents)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Qty = qty;
        UnitPriceCents = unitPriceCents;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Qty { get; private set; }
    public int UnitPriceCents { get; private set; }
}

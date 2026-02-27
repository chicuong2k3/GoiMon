namespace GoiMon.Api.Legacy.Domain.Entities;

using System.Linq;
using GoiMon.Api.Domain;

/// <summary>
/// Represents a product combo (bundle) composed of multiple products.
/// </summary>
public class ProductCombo : AggregateRoot
{
    private ProductCombo() { Items = new List<ProductComboItem>(); }

    public ProductCombo(Guid id, string name, int priceCents)
    {
        Id = id;
        Name = name;
        PriceCents = priceCents;
        Items = new List<ProductComboItem>();
    }

    public string? Name { get; private set; }
    public int PriceCents { get; private set; }
    public List<ProductComboItem> Items { get; private set; }

    public void AddItem(Guid productId, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        var item = new ProductComboItem(Guid.NewGuid(), Id, productId, qty);
        Items.Add(item);
    }

    /// <summary>
    /// Update the display name of the combo.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        Name = name.Trim();
    }

    /// <summary>
    /// Update the price in cents for the combo.
    /// </summary>
    public void UpdatePrice(int priceCents)
    {
        if (priceCents < 0) throw new ArgumentOutOfRangeException(nameof(priceCents));
        PriceCents = priceCents;
    }

    /// <summary>
    /// Remove all items from the combo.
    /// </summary>
    public void ClearItems()
    {
        Items.Clear();
    }

    /// <summary>
    /// Remove a single item from the combo by id.
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item is not null)
            Items.Remove(item);
    }
}

public class ProductComboItem
{
    private ProductComboItem() { }

    public ProductComboItem(Guid id, Guid comboId, Guid productId, int qty)
    {
        Id = id;
        ComboId = comboId;
        ProductId = productId;
        Qty = qty;
    }

    public Guid Id { get; private set; }
    public Guid ComboId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Qty { get; private set; }
}

namespace GoiMon.Api.Domain.Entities;

using System.Linq;
using GoiMon.Api.Domain;

/// <summary>
/// Represents a product combo (bundle) composed of multiple products.
/// </summary>
public class ProductCombo : AggregateRoot
{
    private ProductCombo() { Items = new List<ProductComboItem>(); }

    public ProductCombo(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name?.Trim().ToLowerInvariant();
        Price = price;
        Items = new List<ProductComboItem>();
    }

    public string? Name { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public List<ProductComboItem> Items { get; private set; }

    public void AddItem(Guid productId, int qty, Guid? variantId = null)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        var item = new ProductComboItem(Guid.NewGuid(), Id, productId, qty, variantId);
        Items.Add(item);
        AddDomainEvent(new Events.ComboItemAddedEvent(Id, item.Id, productId, qty));
    }

    /// <summary>
    /// Update the display name of the combo.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        Name = name.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Update the price for the combo.
    /// </summary>
    public void UpdatePrice(decimal price)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        Price = price;
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

    /// <summary>
    /// Update the quantity of an existing item in the combo.
    /// </summary>
    public void UpdateItem(Guid itemId, int qty)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        item?.UpdateQty(qty);
    }

    public void UpdateImage(string? imageUrl)
    {
        var old = ImageUrl;
        ImageUrl = imageUrl?.Trim();
        AddDomainEvent(new Events.ComboImageUpdatedEvent(Id, old, ImageUrl));
    }

    public void MarkDeleted()
    {
        AddDomainEvent(new Events.ComboDeletedEvent(Id, ImageUrl));
    }
}

public class ProductComboItem
{
    private ProductComboItem() { }

    public ProductComboItem(Guid id, Guid comboId, Guid productId, int qty, Guid? variantId = null)
    {
        Id = id;
        ComboId = comboId;
        ProductId = productId;
        VariantId = variantId;
        Qty = qty;
    }

    public Guid Id { get; private set; }
    public Guid ComboId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int Qty { get; private set; }

    public void UpdateQty(int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        Qty = qty;
    }
}

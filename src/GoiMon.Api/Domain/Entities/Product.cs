namespace GoiMon.Api.Domain.Entities;

using GoiMon.Api.Domain;

public class Product : AggregateRoot
{
    // EF Core requires a parameterless constructor; keep it private for DDD control
    private Product() { }

    public Product(Guid id, string name, decimal price, Guid? categoryId = null, string? description = null)
    {
        Id = id;
        Name = name?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(name));
        Price = price;
        CategoryId = categoryId;
        Description = description?.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string? Description { get; private set; }

    // Domain behaviors
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Name cannot be empty", nameof(newName));
        Name = newName.Trim().ToLowerInvariant();
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new ArgumentOutOfRangeException(nameof(newPrice));
        var old = Price;
        Price = newPrice;
        AddDomainEvent(new Events.ProductPriceChangedEvent(Id, old, newPrice));
    }

    public void ChangeCategory(Guid? newCategoryId)
    {
        var old = CategoryId;
        CategoryId = newCategoryId;
        AddDomainEvent(new Events.ProductCategoryChangedEvent(Id, old, newCategoryId));
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        AddDomainEvent(new Events.ProductDescriptionUpdatedEvent(Id, Description));
    }
}

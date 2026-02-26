namespace GoiMon.Api.Domain.Entities;

using GoiMon.Api.Domain;

public class Product : AggregateRoot
{
    // EF Core requires a parameterless constructor; keep it private for DDD control
    private Product() { }

    public Product(Guid id, string name, int priceCents, string category)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PriceCents = priceCents;
        Category = category ?? string.Empty;
    }

    public string Name { get; private set; } = string.Empty;
    public int PriceCents { get; private set; }
    public string Category { get; private set; } = string.Empty;

    // Domain behaviors
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Name cannot be empty", nameof(newName));
        Name = newName.Trim();
    }

    public void ChangePrice(int newPriceCents)
    {
        if (newPriceCents < 0) throw new ArgumentOutOfRangeException(nameof(newPriceCents));
        PriceCents = newPriceCents;
    }

    public void ChangeCategory(string newCategory)
    {
        Category = newCategory ?? string.Empty;
    }
}

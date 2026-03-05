namespace GoiMon.Api.Domain.Entities;

using GoiMon.Api.Domain;

public class Category : AggregateRoot
{
    private Category() { }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        Name = name.Trim();
    }
}

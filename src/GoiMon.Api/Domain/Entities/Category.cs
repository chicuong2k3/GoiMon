namespace GoiMon.Api.Domain.Entities;

using GoiMon.Api.Domain;

public class Category : AggregateRoot, IMultiTenant
{
    private Category() { }

    public Category(Guid id, Guid tenantId, string name)
    {
        Id = id;
        TenantId = tenantId;
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
    }

    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        Name = name.Trim();
    }
}

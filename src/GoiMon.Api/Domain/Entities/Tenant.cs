namespace GoiMon.Api.Domain.Entities;

using GoiMon.Api.Domain;

/// <summary>
/// Represents a merchant/subscriber (Tenant).
/// </summary>
public sealed class Tenant : AggregateRoot
{
    private Tenant() { }

    public Tenant(Guid id, string name, string? slug = null)
    {
        Id = id;
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Slug = slug?.Trim() ?? name.ToLowerInvariant().Replace(" ", "-");
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Display name of the store/merchant.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Unique URL-friendly identifier for the tenant.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Business registration or tax code.
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Primary contact phone.
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Physical address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Indicates if the tenant is allowed to perform operations.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Timestamp when merchant joined.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void Rename(string newName) => Name = newName;
}

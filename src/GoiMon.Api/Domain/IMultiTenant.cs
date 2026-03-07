namespace GoiMon.Api.Domain;

/// <summary>
/// Interface for entities that belong to a specific tenant.
/// </summary>
public interface IMultiTenant
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    Guid TenantId { get; set; }
}

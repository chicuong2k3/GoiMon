namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current TenantId from the execution context.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Ensures a TenantId is present, throwing if not.
    /// </summary>
    Guid GetTenantId() => TenantId ?? throw new InvalidOperationException("Tenant context is missing.");
}

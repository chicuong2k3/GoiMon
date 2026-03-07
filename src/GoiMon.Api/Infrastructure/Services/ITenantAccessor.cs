namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// Provides a process-local tenant id storage usable from singletons and pooled services.
/// Implemented with AsyncLocal so the value flows with async calls.
/// </summary>
public interface ITenantAccessor
{
    Guid? TenantId { get; set; }
}

using System.Security.Claims;

namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// Retrieves the current tenant ID from the HTTP context's user claims.
/// </summary>
public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var tenantIdClaim = user?.FindFirst("tid")?.Value;

            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return tenantId;
            }

            return null;
        }
    }
}

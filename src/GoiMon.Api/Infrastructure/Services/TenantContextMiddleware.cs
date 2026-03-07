using System.Security.Claims;

namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// Middleware that populates the <see cref="ITenantAccessor"/> from the current HTTP user claims.
/// Runs after authentication so HttpContext.User is available.
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantAccessor accessor)
    {
        try
        {
            var user = context.User;
            var tenantClaim = user?.FindFirst("tid")?.Value;
            if (Guid.TryParse(tenantClaim, out var tid))
            {
                accessor.TenantId = tid;
            }
            else
            {
                accessor.TenantId = null;
            }
        }
        catch
        {
            accessor.TenantId = null;
        }

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            // Clear after request to avoid leak across async contexts
            accessor.TenantId = null;
        }
    }
}

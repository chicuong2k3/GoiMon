using System.Threading.Tasks;
using GoiMon.Tenancy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GoiMon.Tenancy.Tests
{
    public class TenantMiddlewareTests
    {
        [Fact]
        public async Task Middleware_Sets_Tenant_From_Header()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["x-tenant-id"] = "tenant-123";

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new TenantMiddleware(next);

            await middleware.InvokeAsync(context);

            Assert.True(context.Items.ContainsKey("TenantId"));
            Assert.Equal("tenant-123", context.Items["TenantId"]);
        }

        [Fact]
        public async Task Middleware_Prefers_Jwt_Claim_Over_Header()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["x-tenant-id"] = "tenant-header";
            var identity = new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim("tenant_id", "tenant-claim")
            });
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new TenantMiddleware(next);

            await middleware.InvokeAsync(context);

            Assert.Equal("tenant-claim", context.Items["TenantId"]);
        }
    }
}

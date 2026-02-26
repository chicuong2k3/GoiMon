using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GoiMon.Tenancy
{
    // Simple middleware that resolves tenant id from JWT claim or header and stores it in HttpContext.Items
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Prefer JWT claim `tenant_id`, fallback to header `x-tenant-id`
            string tenantId = context.User?.FindFirst("tenant_id")?.Value
                              ?? context.Request.Headers["x-tenant-id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(tenantId))
            {
                context.Items["TenantId"] = tenantId;
            }

            await _next(context);
        }
    }

    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TenantMiddleware>();
        }

        // HotChocolate HTTP interceptor registration helper
        public static IServiceCollection AddGraphQlTenantInterceptor(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddGraphQLServer() // note: in real app configure types separately
                .AddHttpRequestInterceptor<GraphQlTenantInterceptorPlaceholder>();
            return services;
        }
    }

    // Placeholder interceptor type to show intent. Replace with actual interceptor implementation in Program.cs
    public class GraphQlTenantInterceptorPlaceholder : HotChocolate.AspNetCore.HttpRequestInterceptor
    {
        public override ValueTask OnCreateAsync(HotChocolate.AspNetCore.HttpRequestInterceptorContext context)
        {
            var http = context.GetHttpContext();
            if (http != null && http.Items.TryGetValue("TenantId", out var t) && t is string tenantId)
            {
                context.ContextData = context.ContextData.SetItem("tenantId", tenantId);
            }
            return ValueTask.CompletedTask;
        }
    }
}

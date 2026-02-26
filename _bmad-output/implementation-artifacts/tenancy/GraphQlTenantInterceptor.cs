using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using HotChocolate.Execution;
using HotChocolate.AspNetCore;

namespace GoiMon.Tenancy
{
    // HotChocolate HTTP request interceptor that copies tenant id from HttpContext.Items
    // into the GraphQL request builder so resolvers can access it via context.ContextData["tenantId"].
    public class GraphQlTenantInterceptor : IHttpRequestInterceptor
    {
        public async ValueTask OnCreateAsync(HttpContext httpContext, IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            if (httpContext is not null && httpContext.Items.TryGetValue("TenantId", out var t) && t is string tenantId)
            {
                // store tenant id in the request builder properties/context
                requestBuilder.SetProperty("tenantId", tenantId);
            }

            await ValueTask.CompletedTask;
        }
    }
}

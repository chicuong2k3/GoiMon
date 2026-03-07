using GoiMon.Api.Domain.Entities;
using GoiMon.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GoiMon.Api.Features.Tenants;

[ExtendObjectType("Mutation")]
public class TenantMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<Tenant> InitializeMerchantStore(
        string storeName,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        // 1. Lấy User ID từ Claims
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Unauthorized. Please login first.")
                .SetCode("AUTH_REQUIRED")
                .Build());
        }

        // 2. Kiểm tra User hiện tại
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new GraphQLException("User not found.");
        }

        // 3. Tạo Tenant mới
        var newTenantId = Guid.NewGuid();
        var tenant = new Tenant(newTenantId, storeName);
        db.Tenants.Add(tenant);

        // 4. Cập nhật User sang Tenant mới
        user.UpdateTenant(newTenantId);

        // 5. Seed dữ liệu mẫu cho Tenant mới (để test ngay)
        var defaultCategory = new Category(Guid.NewGuid(), "General", newTenantId);
        db.Categories.Add(defaultCategory);

        var sampleProduct = new Product(
            Guid.NewGuid(), 
            "Sample Product", 
            10000m, 
            defaultCategory.Id, 
            "Auto-generated sample product",
            newTenantId);
        db.Products.Add(sampleProduct);

        var sampleTable = new TableSlot(Guid.NewGuid(), "T1", "Table 01", 4, newTenantId);
        db.TableSlots.Add(sampleTable);

        // Lưu tất cả thay đổi
        await db.SaveChangesAsync();

        return tenant;
    }
}

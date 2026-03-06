using System.Security.Claims;
using GoiMon.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace GoiMon.Api.Infrastructure.Authorization;

public static class AuthorizationConfig
{
    public static void AddPolicyMatrix(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Order Ops
            options.AddPolicy(Policies.Order.EditPrePayment, policy =>
                policy.RequireRole(
                    UserRole.Cashier.ToString(),
                    UserRole.Supervisor.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString()));

            // For Approval-Required Actions, the base role must be Supervisor+ initially.
            // (The token of the supervisor will be required)
            options.AddPolicy(Policies.Order.EditPostPayment, policy =>
                policy.RequireRole(
                    UserRole.Supervisor.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString()));

            options.AddPolicy(Policies.Order.Void, policy =>
                policy.RequireRole(
                    UserRole.Supervisor.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString()));

            options.AddPolicy(Policies.Order.HardDelete, policy =>
                policy.RequireRole(
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString()));

            // Print Ops
            options.AddPolicy(Policies.Order.Reprint, policy =>
                policy.RequireRole(
                    UserRole.Cashier.ToString(),
                    UserRole.Supervisor.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString(),
                    UserRole.Accountant.ToString()));

            // Shift Ops
            options.AddPolicy(Policies.Shift.Close, policy =>
                policy.RequireRole(
                    UserRole.Cashier.ToString(),
                    UserRole.Supervisor.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString()));

            // Inventory
            options.AddPolicy(Policies.Inventory.Adjust, policy =>
                policy.RequireRole(
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString()));

            // Reporting
            options.AddPolicy(Policies.Reports.View, policy =>
                policy.RequireRole(
                    UserRole.Supervisor.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Owner.ToString(),
                    UserRole.Accountant.ToString()));
        });
    }
}

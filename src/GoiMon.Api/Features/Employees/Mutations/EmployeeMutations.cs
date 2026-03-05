using GoiMon.Api.Features.Employees.Models;

namespace GoiMon.Api.Features.Employees.Mutations;

[ExtendObjectType("Mutation")]
public sealed class EmployeeMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<User> CreateEmployee(CreateEmployeeInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = input.FirstName?.Trim(),
            LastName = input.LastName?.Trim(),
            Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
            Role = input.Role,
            IsVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<User?> UpdateEmployee(UpdateEmployeeInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (user is null)
        {
            return null;
        }

        user.Email = input.Email.Trim().ToLowerInvariant();
        user.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        user.FirstName = string.IsNullOrWhiteSpace(input.FirstName) ? null : input.FirstName.Trim();
        user.LastName = string.IsNullOrWhiteSpace(input.LastName) ? null : input.LastName.Trim();

        if (user.Role != input.Role)
        {
            if (user.Role == Domain.Enums.UserRole.Owner && input.Role != Domain.Enums.UserRole.Owner)
            {
                var activeOwners = await db.Users.CountAsync(x => x.IsActive && x.Role == Domain.Enums.UserRole.Owner);
                if (activeOwners <= 1)
                {
                    throw new GraphQLException(
                        ErrorBuilder.New()
                            .SetMessage("Cannot demote the last active owner.")
                            .SetCode("LAST_OWNER_DEMOTION_BLOCKED")
                            .Build());
                }
            }

            user.ChangeRole(input.Role);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return user;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<User?> ToggleEmployeeStatus(ToggleEmployeeStatusInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (user is null)
        {
            return null;
        }

        if (!input.IsActive && user.Role == Domain.Enums.UserRole.Owner)
        {
            var activeOwners = await db.Users.CountAsync(x => x.IsActive && x.Role == Domain.Enums.UserRole.Owner);
            if (activeOwners <= 1)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Cannot deactivate the last active owner.")
                        .SetCode("LAST_OWNER_DEACTIVATION_BLOCKED")
                        .Build());
            }
        }

        if (input.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await db.SaveChangesAsync();
        return user;
    }
}

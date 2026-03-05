namespace GoiMon.Api.Features.Employees.Models;

public record CreateEmployeeInput(
    string Email,
    string? Phone,
    string? FirstName,
    string? LastName,
    Domain.Enums.UserRole Role);

public record UpdateEmployeeInput(
    Guid Id,
    string Email,
    string? Phone,
    string? FirstName,
    string? LastName,
    Domain.Enums.UserRole Role);

public record ToggleEmployeeStatusInput(Guid Id, bool IsActive);

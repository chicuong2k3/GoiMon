namespace GoiMon.Api.Domain.Entities;

/// <summary>
/// Order status values.
/// Stored in the database as strings via EF Core conversion.
/// </summary>
public enum OrderStatus
{
    Open,
    Completed,
    Paid,
    Cancelled
}

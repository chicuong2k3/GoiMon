namespace GoiMon.Shared.Models;

/// <summary>
/// DTO representing an order line item enriched with product information.
/// Keep this model free of GraphQL concerns so it can be shared across projects.
/// </summary>
public partial class OrderItemDto
{
    /// <summary>
    /// Identifier for the order item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Referenced product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Resolved product name (may be null if product not found).
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Quantity ordered for this line item.
    /// </summary>
    public int Qty { get; set; }

    /// <summary>
    /// Unit price for the line item.
    /// </summary>
    public decimal UnitPrice { get; set; }
}

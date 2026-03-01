namespace GoiMon.Api.Domain.Events;

public record OrderCreatedEvent(Guid OrderId);
public record OrderItemAddedEvent(Guid OrderId, Guid OrderItemId, Guid ProductId, int Qty);
public record OrderCompletedEvent(Guid OrderId);

public record ProductPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice);
public record ProductCategoryChangedEvent(Guid ProductId, Guid? OldCategoryId, Guid? NewCategoryId);
public record ProductDescriptionUpdatedEvent(Guid ProductId, string? Description);
public record ProductImageUpdatedEvent(Guid ProductId, string? OldImageUrl, string? NewImageUrl);
public record ProductDeletedEvent(Guid ProductId, string? ImageUrl);

public record ComboItemAddedEvent(Guid ComboId, Guid ItemId, Guid ProductId, int Qty);
public record ComboImageUpdatedEvent(Guid ComboId, string? OldImageUrl, string? NewImageUrl);
public record ComboDeletedEvent(Guid ComboId, string? ImageUrl);

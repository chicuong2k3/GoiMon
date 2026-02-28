namespace GoiMon.Api.Domain.Events;

public record OrderCreatedEvent(Guid OrderId);
public record OrderItemAddedEvent(Guid OrderId, Guid OrderItemId, Guid ProductId, int Qty);
public record OrderCompletedEvent(Guid OrderId);

public record ProductPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice);
public record ProductCategoryChangedEvent(Guid ProductId, Guid? OldCategoryId, Guid? NewCategoryId);
public record ProductDescriptionUpdatedEvent(Guid ProductId, string? Description);

public record ComboItemAddedEvent(Guid ComboId, Guid ItemId, Guid ProductId, int Qty);

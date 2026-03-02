namespace GoiMon.Client.State;

public record UiCacheState(
    CategoryPageCache? Categories,
    ProductPageCache? Products,
    ComboPageCache? Combos,
    OrderPageState? Orders,
    CheckoutPageState? Checkout)
{
    public static UiCacheState Initial => new(null, null, null, null, null);
}

public record CategoryPageCache(
    string Search,
    int Skip,
    int Take,
    int Total,
    IReadOnlyList<CategoryListItem> Items,
    IReadOnlyList<Guid> SelectedIds);

public record CategoryListItem(Guid Id, string Name);

public record ProductPageCache(
    string Search,
    Guid? FilterCategoryId,
    int Skip,
    int Take,
    int Total,
    IReadOnlyList<ProductListItem> Items,
    IReadOnlyList<CategoryLookupItem> Categories,
    IReadOnlyList<Guid> SelectedIds);

public record ProductListItem(
    Guid Id,
    string Name,
    decimal Price,
    string? UnitName,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    string? ImageUrl = null);

public record CategoryLookupItem(Guid Id, string Name);

public record ComboPageCache(
    string Search,
    int Skip,
    int Take,
    int Total,
    IReadOnlyList<ComboListItem> Items,
    IReadOnlyList<ProductLookupItem> Products,
    IReadOnlyList<Guid> SelectedIds);

public record ComboListItem(
    Guid Id,
    string? Name,
    decimal Price,
    IReadOnlyList<ComboItemListItem> Items,
    string? ImageUrl = null);

public record ComboItemListItem(Guid Id, Guid ProductId, string? ProductName, int Qty);

public record ProductLookupItem(Guid Id, string Name);

public record OrderPageState(
    string ActiveTab,
    Guid? SelectedOrderId,
    IReadOnlyList<OrderListItem> Items);

public record OrderListItem(
    Guid Id,
    string Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemSnapshot> Items);

public record OrderItemSnapshot(
    Guid Id,
    string ProductName,
    string? UnitName,
    int Qty,
    decimal UnitPrice,
    IReadOnlyList<OrderItemModifierSnapshot>? Modifiers = null)
{
    public IReadOnlyList<OrderItemModifierSnapshot> SafeModifiers => Modifiers ?? [];
}

public record OrderItemModifierSnapshot(
    Guid Id,
    string GroupName,
    string OptionName,
    int Qty,
    decimal UnitDeltaPrice);

public record ProductConfiguratorState(
    Guid ProductId,
    Guid? SelectedVariantId,
    IReadOnlyDictionary<Guid, int> SelectedOptionQuantities,
    decimal EstimatedUnitPrice);

public record CheckoutPageState(
    string Search,
    string SelectedCategory,
    IReadOnlyList<CheckoutProductListItem> Products,
    IReadOnlyList<CheckoutCartLineItem> Cart);

public record CheckoutProductListItem(
    Guid Id,
    string Name,
    decimal Price,
    string? UnitName,
    string? Description,
    string? ImageUrl,
    string CategoryName,
    int ActiveVariantCount,
    int ActiveModifierGroupCount);

public record CheckoutCartLineItem(
    Guid ProductId,
    string ProductName,
    Guid? VariantId,
    string? VariantName,
    string? UnitName,
    int Quantity,
    decimal UnitPrice,
    IReadOnlyList<CheckoutCartModifierItem> Modifiers);

public record CheckoutCartModifierItem(Guid OptionId, string OptionName, int Qty, decimal PriceDelta);

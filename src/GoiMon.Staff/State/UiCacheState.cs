namespace GoiMon.Staff.State;

public record AuthUiState(Guid? OtpPendingUserId, string DeliveryMethod)
{
    public static AuthUiState Initial => new(null, "email");
}

public record CategoriesUiState(CategoryPageCache? Cache)
{
    public static CategoriesUiState Initial => new((CategoryPageCache?)null);
}

public record ProductsUiState(ProductPageCache? Cache)
{
    public static ProductsUiState Initial => new((ProductPageCache?)null);
}

public record CombosUiState(ComboPageCache? Cache)
{
    public static CombosUiState Initial => new((ComboPageCache?)null);
}

public record OrdersUiState(OrderPageState? Cache)
{
    public static OrdersUiState Initial => new((OrderPageState?)null);
}

public record CheckoutUiState(CheckoutPageState? Cache)
{
    public static CheckoutUiState Initial => new((CheckoutPageState?)null);
}

public record EmployeesUiState(EmployeePageCache? Cache)
{
    public static EmployeesUiState Initial => new((EmployeePageCache?)null);
}

public record TablesUiState(TablePageCache? Cache)
{
    public static TablesUiState Initial => new((TablePageCache?)null);
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

public record ComboItemListItem(Guid Id, Guid ProductId, string? ProductName, Guid? VariantId, string? VariantName, int Qty);

public record ProductLookupItem(Guid Id, string Name);

public record OrderPageState(
    string ActiveTab,
    Guid? SelectedOrderId,
    IReadOnlyList<OrderListItem> Items);

public record OrderListItem(
    Guid Id,
    Guid? TableSlotId,
    string Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemSnapshot> Items);

public record OrderItemSnapshot(
    Guid Id,
    string ProductName,
    string? UnitName,
    Guid? ComboId,
    string? ComboName,
    int Qty,
    decimal UnitPrice,
    IReadOnlyList<OrderItemModifierSnapshot>? Modifiers = null)
{
    public IReadOnlyList<OrderItemModifierSnapshot> SafeModifiers => Modifiers ?? System.Array.Empty<OrderItemModifierSnapshot>();
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
    string Mode,
    string Search,
    string SelectedCategory,
    IReadOnlyList<CheckoutProductListItem> Products,
    IReadOnlyList<CheckoutComboListItem> Combos,
    IReadOnlyList<CheckoutCartLineItem> Cart,
    string ServiceMode = "takeaway",
    Guid? TableSlotId = null);

public record TablePageCache(
    string Search,
    int Skip,
    int Take,
    int Total,
    IReadOnlyList<TableSlotListItem> Items);

public record TableSlotListItem(
    Guid Id,
    string Code,
    string Name,
    int Capacity,
    bool IsActive,
    string CurrentState,
    DateTimeOffset UpdatedAt);

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
    Guid? ProductId,
    string DisplayName,
    Guid? ComboId,
    string? ComboName,
    Guid? VariantId,
    string? VariantName,
    string? UnitName,
    int Quantity,
    decimal UnitPrice,
    IReadOnlyList<CheckoutCartModifierItem> Modifiers);

public record CheckoutCartModifierItem(Guid OptionId, string OptionName, int Qty, decimal PriceDelta);

public record CheckoutComboListItem(
    Guid Id,
    string? Name,
    decimal Price,
    string? ImageUrl,
    IReadOnlyList<CheckoutComboItemListItem> Items);

public record CheckoutComboItemListItem(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    Guid? VariantId,
    string? VariantName,
    int Qty);

public record EmployeePageCache(
    string Search,
    int Skip,
    int Take,
    int Total,
    string SortOption,
    IReadOnlyList<EmployeeListItem> Items,
    IReadOnlyList<Guid> SelectedIds);

public record EmployeeListItem(
    Guid Id,
    string Email,
    string? Phone,
    string? FirstName,
    string? LastName,
    string Role,
    bool IsActive,
    bool IsVerified,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

namespace GoiMon.Client.State;

public record UiCacheState(
    CategoryPageCache? Categories,
    ProductPageCache? Products,
    ComboPageCache? Combos)
{
    public static UiCacheState Initial => new(null, null, null);
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

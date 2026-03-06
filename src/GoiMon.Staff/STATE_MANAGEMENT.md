# GoiMon.Client — State Management & Performance Guide

This guide defines the official state-management strategy for `GoiMon.Client` using `EasyAppDev.Blazor.Store`.

## Goals

- Reduce unnecessary re-renders
- Keep state modular and maintainable
- Make feature-level ownership clear for parallel development

## Current Store Topology

Register focused stores in `Program.cs`:

- `CategoriesUiState`
- `ProductsUiState`
- `CombosUiState`
- `OrdersUiState`
- `CheckoutUiState`

Each store owns only its domain cache and should not include cross-domain payload.

## Why Multi-Store (Instead of Monolithic UI Store)

A monolithic store causes broad update propagation and creates coupling between unrelated screens.
Focused stores provide:

- smaller update scope
- easier reasoning and testing
- lower risk of accidental regressions
- better team parallelism

## Choosing Component Base

Choose the page/component base class by interaction pattern:

- `StoreComponentWithUtilities<TState>` for interaction-heavy screens with local UI state (dialogs, selection state, tab/panel state, form drafts).
- `SelectorStoreComponent<TState>` for read-heavy components where selected store slices are the main render trigger and local UI state is minimal.

### Selector Safety Rule

Selector render-gating can block local UI updates when selected store values do not change.
Do not use selector-based pages for dialog-heavy CRUD screens unless all local interaction triggers are explicitly represented in the render trigger strategy.

```razor
@inherits SelectorStoreComponent<OrdersUiState>

@code {
    protected override object SelectState(OrdersUiState state)
        => (state.Cache?.ActiveTab, state.Cache?.SelectedOrderId, state.Cache?.Items.Count);
}
```

Recommended selector patterns:

1. Single property: `s => s.Cache?.Search`
2. Tuple: `s => (s.Cache?.Search, s.Cache?.SelectedCategory)`
3. Derived record for expensive computed values
4. Filtered collections only when necessary

## Applied Coverage (Current Baseline)

Interaction-heavy pages currently use `StoreComponentWithUtilities<TState>`:

- `Pages/Categories.razor` (`StoreComponentWithUtilities<CategoriesUiState>`)
- `Pages/Products.razor` (`StoreComponentWithUtilities<ProductsUiState>`)
- `Pages/Combos.razor` (`StoreComponentWithUtilities<CombosUiState>`)
- `Pages/Checkout.razor` (`StoreComponentWithUtilities<CheckoutUiState>`)
- `Pages/Orders.razor` (`StoreComponentWithUtilities<OrdersUiState>`)

Use selector-based components selectively for read-heavy views/components.

## Update Patterns

- Prefer one combined update over multiple sequential updates.
- Debounce high-frequency updates (search input, scroll-driven loads).
- Use domain-scoped action names, e.g.:
  - `cache.categories`
  - `cache.products`
  - `cache.combos`
  - `cache.orders`
  - `cache.checkout`

## Debounced Updates (`UpdateDebounced` Pattern)

`UpdateDebounced` is available on `StoreComponentWithUtilities<TState>`.

Mandatory policy in GoiMon.Client: always apply debounced updates for every feasible high-frequency input/event path.

Preferred approach:

1. Use `UpdateDebounced` when updating store-backed state slices.
2. Use `IDebounceManager` for explicit key-based debounce flows.
3. Execute UI-bound logic via `InvokeAsync(...)` when needed.

Example:

```razor
@inject IDebounceManager DebounceManager

@code {
  private Task OnSearchChangedAsync(string value)
  {
    _search = value;
    _skip = 0;

    return DebounceManager.Debounce(
      "products.search.load",
      async () => await InvokeAsync(async () => await LoadAsync()),
      300);
  }
}
```

Delay guidance:

- Search/filter: 200-400ms
- Form validation: 300-500ms
- Auto-save/persist: 1000ms+

Applied debounce coverage (current baseline):

- `Pages/Categories.razor` (search)
- `Pages/Products.razor` (search + price range filters)
- `Pages/Combos.razor` (search + price range filters)
- `Pages/Checkout.razor` (search state persistence)
- `Pages/Orders.razor` (debounced cache persistence)

## Throttled Updates (`UpdateThrottled` Pattern)

`UpdateThrottled` is available on `StoreComponentWithUtilities<TState>`.

Preferred approach: use `UpdateThrottled` directly when suitable; use `IThrottleManager` for explicit key-based throttling of continuous/high-frequency event streams.

Example:

```razor
@inject IThrottleManager ThrottleManager

@code {
  private Task OnScrollAsync(EventArgs _)
    => ThrottleManager.Throttle(
      "orders.scroll.load-next",
      async () => await InvokeAsync(LoadNextPageAsync),
      100,
      leading: true);
}
```

Interval guidance:

- Scroll/mouse tracking: ~100ms
- Resize/layout recalculation: 150-250ms
- Realtime streams/analytics bursts: 200-500ms

Mandatory policy in GoiMon.Client: always apply throttling for continuous high-frequency event paths wherever feasible.

Applied throttle coverage (current baseline):

- `Pages/Orders.razor` (`@onscroll` load-next handling)

Audit reference:

- `PERFORMANCE_EVENT_CHECKLIST.md` (high-frequency event inventory and status)

## LazyLoad / Request Deduplication (`ILazyCache` Pattern)

Use lazy-load caching for repeated read paths to avoid duplicate API calls and improve perceived performance.

In state-driven pages/components, use `ILazyCache.GetOrLoadAsync(...)` for repeated read/query paths.

Example:

```csharp
var products = await LazyCache.GetOrLoadAsync(
    "checkout.menu.products",
    FetchCheckoutProductsAsync,
    TimeSpan.FromMinutes(2));
```

Guidelines:

- Use stable, descriptive cache keys (`{feature}.{resource}.{scope}`)
- Choose TTL by volatility:
  - 5-10s: tab/count summaries
  - 1-2m: frequently refreshed list/menu data
  - 5m+: lookup/reference lists
- Invalidate (`RemoveAsync`) on force-refresh or after mutations when stale risk is high.

Mandatory policy in GoiMon.Client: always apply lazy-load caching for repeated read/query paths wherever feasible.

Applied lazy-load coverage (current baseline):

- `Pages/Products.razor` — categories lookup (`products.lookup.categories`)
- `Pages/Combos.razor` — products lookup (`combos.lookup.products`)
- `Pages/Checkout.razor` — menu products/combos (`checkout.menu.products`, `checkout.menu.combos`)
- `Pages/Orders.razor` — tab total counts (`orders.tab.count.{tab}`)

## ExecuteCachedAsync (Fetch + State-Update Deduplication)

Use `IAsyncActionExecutor<TState>.ExecuteCachedAsync(...)` when concurrent callers may hit the same read path and the screen also writes store state.

Why:

- Deduplicates network fetch across concurrent callers
- Deduplicates loading/success/error callbacks (first caller only)
- Prevents `N×2` duplicated store writes when multiple callers share one `cacheKey`

State-driven pages can use executor directly:

```razor
@inject IAsyncActionExecutor<ProductsUiState> AsyncExecutor

@code {
  var categories = await AsyncExecutor.ExecuteCachedAsync(
    "products.lookup.categories",
    FetchAllCategoriesFromApiAsync,
    loading: state => state,
    success: (state, result) => state with { Cache = BuildCacheWithCategories(state.Cache, result) },
    error: (state, _) => state,
    cacheFor: TimeSpan.FromMinutes(5));
}
```

Invalidation guidance:

- Use `InvalidateCacheAsync(cacheKey)` after mutation paths that stale the specific cached entry
- Use `InvalidateCacheByPrefixAsync(prefix)` for grouped invalidation after bulk operations
- Use `ClearCacheAsync()` for global reset/logout scenarios

Applied `ExecuteCachedAsync` coverage (current baseline):

- `Pages/Products.razor` — categories lookup/state update (`products.lookup.categories`)
- `Pages/Combos.razor` — products lookup/state update (`combos.lookup.products`)
- `Pages/Checkout.razor` — menu products/combos lookup + cache snapshot writes (`checkout.menu.products`, `checkout.menu.combos`)

## Derived State Guidance

Do not store values that can be computed from source data (counts, totals, filtered views) unless needed for persistence/interoperability.
Use computed properties or derived records instead.

## Optimistic Update Guidance

Use optimistic updates only when:

- operation success rate is high
- rollback is simple and deterministic
- operation is non-critical

Avoid optimistic updates for irreversible critical operations.

## Performance Checklist (Before Merge)

- [ ] Store writes are scoped to correct domain store
- [ ] High-frequency inputs are debounced/throttled
- [ ] No unnecessary cross-domain state dependencies
- [ ] Selector pattern used where component render pressure is high
- [ ] Behavior verified after optimization (no UX/data regression)

## Notes for LLM Agents

When generating or refactoring UI state code:

1. Never re-introduce a monolithic cache store for multiple features.
2. Choose component base by interaction type; avoid selector gating on dialog-heavy CRUD screens.
3. Keep state immutable, minimal, and domain-focused.
4. Update documentation and `copilot-instructions.md` when introducing new store patterns.

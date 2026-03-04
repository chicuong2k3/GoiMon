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

## Selector Pattern (Granular Re-render)

When a component binds to store state, use selector-based subscription.

Mandatory rule in GoiMon.Client: all state-driven pages/screens must inherit `SelectorStoreComponent<TState>`.

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

Selector-based store subscriptions are already applied to:

- `Pages/Categories.razor` (`SelectorStoreComponent<CategoriesUiState>`)
- `Pages/Products.razor` (`SelectorStoreComponent<ProductsUiState>`)
- `Pages/Combos.razor` (`SelectorStoreComponent<CombosUiState>`)
- `Pages/Checkout.razor` (`SelectorStoreComponent<CheckoutUiState>`)
- `Pages/Orders.razor` (`SelectorStoreComponent<OrdersUiState>`)

Any new state-driven page should follow this same pattern.

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

Because GoiMon pages use `SelectorStoreComponent<TState>` for granular re-rendering, the equivalent debounced pattern is:

1. Inject `IDebounceManager`
2. Debounce expensive update/load callbacks
3. Execute UI-bound logic via `InvokeAsync(...)`

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
2. Prefer selector-based subscriptions for state-driven components.
3. Keep state immutable, minimal, and domain-focused.
4. Update documentation and `copilot-instructions.md` when introducing new store patterns.

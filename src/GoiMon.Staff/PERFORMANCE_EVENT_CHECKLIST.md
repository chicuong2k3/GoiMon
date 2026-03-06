# GoiMon.Staff — High-Frequency Event Checklist

Last updated: 2026-03-04

Purpose: track continuous/high-frequency UI event paths and ensure debounce/throttle is applied wherever feasible.

## Coverage Matrix

| Area | File | Event/Trigger | Current Handling | Status | Note |
|---|---|---|---|---|---|
| Orders | `Pages/Orders.razor` | `@onscroll` order list infinite loading | `IThrottleManager.Throttle("orders.scroll.load-next", 100ms)` | ✅ Applied | Prevents scroll storm and repeated JS metrics/read calls |
| Orders | `Pages/Orders.razor` | Realtime subscription (`OnOrderChanged`) burst updates | `IThrottleManager.Throttle("orders.realtime.merge", 200ms)` | ✅ Applied | Limits merge/render frequency during stream bursts |
| Orders | `Pages/Orders.razor` | Cache persistence during state changes | `IDebounceManager.Debounce("orders.persist", 250ms)` | ✅ Applied | Reduces frequent store writes |
| Categories | `Pages/Categories.razor` | Search input | `IDebounceManager.Debounce("categories.search.load", 300ms)` | ✅ Applied | Debounced query/load |
| Products | `Pages/Products.razor` | Search input | `IDebounceManager.Debounce("products.search.load", 300ms)` | ✅ Applied | Debounced query/load |
| Products | `Pages/Products.razor` | Price min/max filters | `IDebounceManager.Debounce("products.price-min.load" / "products.price-max.load", 300ms)` | ✅ Applied | Debounced filter/load |
| Combos | `Pages/Combos.razor` | Search input | `IDebounceManager.Debounce("combos.search.load", 300ms)` | ✅ Applied | Debounced query/load |
| Combos | `Pages/Combos.razor` | Price min/max filters | `IDebounceManager.Debounce("combos.price-min.load" / "combos.price-max.load", 300ms)` | ✅ Applied | Debounced filter/load |
| Checkout | `Pages/Checkout.razor` | Search state persistence | `IDebounceManager.Debounce("checkout.search.persist", 300ms)` | ✅ Applied | Avoids write churn |

## LazyLoad / Cache-Dedupe Coverage

| Area | File | Read Path | Key | TTL | Status | Note |
|---|---|---|---|---|---|---|
| Products | `Pages/Products.razor` | All categories lookup | `products.lookup.categories` | 5m | ✅ Applied | Dedupe repeated category lookup calls |
| Combos | `Pages/Combos.razor` | All products lookup | `combos.lookup.products` | 5m | ✅ Applied | Dedupe repeated product lookup calls |
| Checkout | `Pages/Checkout.razor` | Product menu load | `checkout.menu.products` | 2m | ✅ Applied | Reuse menu snapshot across repeated loads |
| Checkout | `Pages/Checkout.razor` | Combo menu load | `checkout.menu.combos` | 2m | ✅ Applied | Reuse menu snapshot across repeated loads |
| Orders | `Pages/Orders.razor` | Tab total counts | `orders.tab.count.{tab}` | 10s | ✅ Applied | Avoid repeated count query bursts |

## ExecuteCachedAsync Coverage (Fetch + State-Write Dedupe)

| Area | File | Read Path | Key | TTL | Status | Note |
|---|---|---|---|---|---|---|
| Products | `Pages/Products.razor` | Categories lookup + cache snapshot update | `products.lookup.categories` | 5m | ✅ Applied | Dedupes fetch and store callback updates across concurrent callers |
| Combos | `Pages/Combos.razor` | Products lookup + cache snapshot update | `combos.lookup.products` | 5m | ✅ Applied | Avoids duplicated `cache.combos` writes when concurrent loads occur |
| Checkout | `Pages/Checkout.razor` | Product menu load + cache snapshot update | `checkout.menu.products` | 2m | ✅ Applied | One callback path writes products snapshot per key window |
| Checkout | `Pages/Checkout.razor` | Combo menu load + cache snapshot update | `checkout.menu.combos` | 2m | ✅ Applied | One callback path writes combos snapshot per key window |

## Remaining Findings

- No unthrottled `@onscroll`, `@onmousemove`, `@onresize`, `@onpointermove`, `@onwheel` paths found in current `Pages/*.razor` set.
- Discrete events (combobox selection, button click) remain immediate by design and are not candidates for throttle/debounce.

## Policy

- Always apply debouncing for high-frequency input/value-change paths whenever feasible.
- Always apply throttling for continuous event streams (scroll/mouse/resize/realtime bursts) whenever feasible.
- Always apply lazy-load caching (`ILazyCache.GetOrLoadAsync`) for repeated read/query paths wherever feasible.
- Always prefer `ExecuteCachedAsync` (via `IAsyncActionExecutor<TState>`) over `LazyLoad + manual state update` when the same key is loaded concurrently and store callbacks would otherwise duplicate.
- For dialog-heavy CRUD screens, prefer `StoreComponentWithUtilities<TState>` over selector-based pages to avoid local UI render-gating.
- Re-run this checklist whenever a new high-frequency event path is introduced.

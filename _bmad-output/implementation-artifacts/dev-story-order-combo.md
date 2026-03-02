# 🍱 Dev Story: Order Combo Support

**Status:** 🆕 ready-for-dev  
**Date Created:** 2026-03-03  
**Owner:** Amelia (Developer Agent)  
**User:** Chicuong  
**Story Key:** 3-1-order-combo

---

## Story

As a **staff member**,  
I want to **add a combo to an order** (instead of individual products only),  
so that **customer gets the bundled combo price** and the order receipt clearly shows it was a combo.

---

## Story Scope

Extend the order creation flow (API + Client) to support ordering combos alongside individual products. When a combo is ordered:

- Combo definition supports fixed variant per combo item (`ProductId + VariantId?`), so the same product can appear as different combo SKUs (e.g. milk tea size M vs size L).
- The API resolves the combo with variant-aware items and creates combo order lines with authoritative combo price.
- Each combo order line carries snapshot fields (`ComboId`, `ComboName`) and optional variant detail snapshot for kitchen/readability.
- The Checkout UI allows switching between "Product" and "Combo" ordering modes.
- The Orders UI displays combo-sourced lines with clear variant details (if combo items include variants).

**MVP scope decision:** Fixed variant in combo definition only. Staff does not choose variant at order time for combo items.

**Out of scope:** Runtime variant selection per combo item during checkout (phase 2), combo item modifiers.

---

## Acceptance Criteria

- [ ] **AC1**: `CreateOrderInput` accepts a new `ComboLines` list alongside existing `Lines` list  
- [ ] **AC2**: `ProductComboItem` supports `variantId` (nullable); same product can be represented in different combos by different variants  
- [ ] **AC3**: API validation for combo config: if product has active variants and combo item has no `variantId`, return config error (`COMBO_ITEM_VARIANT_REQUIRED`)  
- [ ] **AC4**: API validates `variantId` belongs to the same product and is active (`COMBO_ITEM_INVALID_VARIANT`)  
- [ ] **AC5**: `OrderItem` entity stores `ComboId` (nullable soft-ref) + `ComboName` snapshot  
- [ ] **AC6**: GraphQL schema exposes `comboId` and `comboName` on `OrderItem` type  
- [ ] **AC7**: Validation errors returned for order create: combo not found, combo has no items, invalid quantity  
- [ ] **AC8**: Checkout page has a "Combo" tab/toggle to browse and order combos  
- [ ] **AC9**: Checkout combo detail clearly shows each combo item and its fixed variant label (e.g. "Trà sữa - Size L")  
- [ ] **AC10**: Checkout page allows setting quantity for a combo and submitting combo orders  
- [ ] **AC11**: Orders page displays combo badge and combo name on combo-sourced lines  
- [ ] **AC12**: Client GraphQL operations updated for combo ordering and combo item variant display  
- [ ] **AC13**: Build succeeds with zero errors on both API and Client  

---

## Task Breakdown

### **TASK 1: Domain Layer — Extend `OrderItem` with Combo Fields** (AC: #5)

- [ ] Add `ComboId` (nullable `Guid?`) to `OrderItem` — soft-reference to source combo (BI/analytics only)
- [ ] Add `ComboName` (nullable `string?`) to `OrderItem` — immutable snapshot of combo name at order time
- [ ] Update `Order.AddItem()` to accept optional `comboId` + `comboName` parameters
- [ ] Update `OrderItem` constructor to accept and store combo fields

**Files:**
- `src/GoiMon.Api/Domain/Entities/Order.cs`

---

### **TASK 2: Infrastructure — EF Configuration + Migration** (AC: #5, #6)

- [ ] Update `OrderItemConfiguration` to map `ComboId` (nullable) and `ComboName` (nullable, max 255)
- [ ] Generate EF migration: `AddComboFieldsToOrderItem`
- [ ] Verify migration applies cleanly

**Files:**
- `src/GoiMon.Api/Infrastructure/Data/Configurations/OrderItemConfiguration.cs`
- `src/GoiMon.Api/Infrastructure/Data/Migrations/<timestamp>_AddComboFieldsToOrderItem.cs`

---

### **TASK 3: API — Extend CreateOrder Mutation for Combo Lines** (AC: #1, #7)

- [ ] Add `CreateOrderComboLineInput` record: `ComboId` (Guid), `Quantity` (int)
- [ ] Extend `CreateOrderInput` to include `ComboLines` (optional list of `CreateOrderComboLineInput`)
- [ ] In `OrderMutations.CreateOrder`:
  - Iterate `input.ComboLines` (if any)
  - Validate combo exists (`db.ProductCombos.Include(c => c.Items)`)
  - Validate combo has items
  - Validate quantity > 0
  - For each combo line: create a single `OrderItem` with:
    - `ProductId = null` (combo is not a single product)
    - `ProductName = combo.Name` (snapshot)
    - `Qty = comboLine.Quantity`
    - `UnitPrice = combo.Price` (authoritative combo price)
    - `ComboId = combo.Id`
    - `ComboName = combo.Name`
  - Return validation errors with codes: `COMBO_NOT_FOUND`, `COMBO_EMPTY`, `INVALID_QTY`
- [ ] Update telemetry tracking to include combo line count

### **TASK 3A: Domain + API — Support Variant in Combo Items (MVP Fixed Variant)** (AC: #2, #3, #4)

- [ ] Add `VariantId` (nullable Guid) to `ProductComboItem`
- [ ] Update `AddComboItemInput` to include optional `VariantId`
- [ ] In `ComboMutations.AddComboItem` and `CreateCombo` item flow:
  - Validate `VariantId` belongs to item `ProductId`
  - Validate variant is active
  - If product has active variants and `VariantId` is null, reject with `COMBO_ITEM_VARIANT_REQUIRED`
  - If variant belongs to another product or inactive, reject with `COMBO_ITEM_INVALID_VARIANT`
- [ ] In combo query payload, expose `variantId` and nested `variant { id name price isActive }`

**Design Decision — Single Summary Line per Combo (keep):**  
Each combo remains ONE order line with `UnitPrice = combo.Price`. Variant details are shown in combo item detail (checkout and kitchen views), not split into multiple receipt lines.

**Files:**
- `src/GoiMon.Api/Features/Orders/OrderMutations.cs`

---

### **TASK 4: API — Update GraphQL Schema** (AC: #6)

- [ ] Ensure `OrderItem` GraphQL type exposes `comboId` and `comboName` fields (auto-exposed by HotChocolate from entity properties)
- [ ] Ensure `CreateOrderInput` schema includes `comboLines` field
- [ ] Additional `CreateOrderComboLineInput` input type exposed
- [ ] Ensure `ProductComboItem` exposes `variantId` + `variant`
- [ ] Ensure combo mutation inputs accept `variantId`
- [ ] Re-export schema and verify with introspection

**Files:**
- Schema auto-generated from code — verify via `dotnet run` + introspection

---

### **TASK 5: Client — Update GraphQL Schema & Operations** (AC: #12)

- [ ] Update `src/GoiMon.Client/schema.graphql`:
  - Add `comboId: UUID` and `comboName: String` to `OrderItem` type
  - Add `input CreateOrderComboLineInput { comboId: UUID!, quantity: Int! }`
  - Add `comboLines: [CreateOrderComboLineInput!]` to `CreateOrderInput`
- [ ] Add `variantId` and `variant` on `ProductComboItem` selection in combo queries/mutations
- [ ] Update `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`:
  - Add `$comboLines` variable to `CreateOrder` mutation
- [ ] Update `src/GoiMon.Client/GraphQL/mutations/ComboMutations.graphql`:
  - Add `$variantId` when adding combo item
- [ ] Update `src/GoiMon.Client/GraphQL/queries/GetOrders.graphql`:
  - Add `comboId` and `comboName` to order item selection
- [ ] Update `src/GoiMon.Client/GraphQL/queries/GetCombos.graphql`:
  - Add combo item variant fields for display
- [ ] Update `src/GoiMon.Client/GraphQL/subscriptions/OrderSubscriptions.graphql`:
  - Add `comboId` and `comboName` to subscription item selection
- [ ] Rebuild StrawberryShake client (`dotnet build`)

**Files:**
- `src/GoiMon.Client/schema.graphql`
- `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`
- `src/GoiMon.Client/GraphQL/queries/GetOrders.graphql`
- `src/GoiMon.Client/GraphQL/subscriptions/OrderSubscriptions.graphql`

---

### **TASK 6: Client — Checkout Page Combo Tab** (AC: #8, #9, #10)

- [ ] Add tab switcher at top of Checkout page: **Sản phẩm** | **Combo**
- [ ] When "Combo" tab is active:
  - Fetch combos using existing `GetCombos` query (already defined in client)
  - Display combo list with name, price, item count badge
  - On combo selection: show combo detail (items list with product + variant label, total price)
  - Quantity input for selected combo
  - "Đặt combo" submit button → calls `CreateOrder` with `comboLines`
- [ ] Combo ordering does NOT use the product configurator (no variant/modifier selection)
- [ ] Support mixed orders: user can add both product lines and combo lines in a single order (future enhancement — v1 is either products or combos per order)

**Files:**
- `src/GoiMon.Client/Pages/Checkout.razor`

---

### **TASK 7: Client — Orders Page Combo Display** (AC: #11)

- [ ] Update `OrderItemSnapshot` record in `UiCacheState.cs` to include `ComboId` and `ComboName`
- [ ] In Orders page: when `ComboName` is not null, display a `BbBadge` with "Combo" label next to the product name
- [ ] Show combo name in the order detail panel

**Files:**
- `src/GoiMon.Client/State/UiCacheState.cs`
- `src/GoiMon.Client/Pages/Orders.razor`

---

## Dev Notes

### Architecture Decisions

1. **Single-line-per-combo model**: A combo is stored as ONE `OrderItem` line with `UnitPrice = combo.Price`. This is simpler than item expansion and matches the real-world receipt pattern. The combo's constituent items are queryable via the `ComboId` → `ProductCombo.Items` relationship for kitchen display purposes.

2. **Authoritative pricing**: The API always reads `combo.Price` from the database — client cannot override. Same pattern as product ordering.

3. **Soft-reference pattern**: `ComboId` on `OrderItem` follows the same pattern as `ProductId` — it's a nullable soft-reference for analytics. If a combo is deleted, historical orders are unaffected because `ComboName` and `UnitPrice` are snapshotted.

4. **Variant-in-combo (MVP fixed)**: Combo item supports `VariantId` selected at combo definition time. This directly solves SKU split use case: Combo 1 = Milk Tea Size M, Combo 2 = Milk Tea Size L.

### Proposed GraphQL Contract (MVP)

```graphql
input ComboItemInput {
  productId: UUID!
  qty: Int!
  variantId: UUID
}

input AddComboItemInput {
  comboId: UUID!
  productId: UUID!
  qty: Int!
  variantId: UUID
}

type ProductComboItem {
  id: UUID!
  comboId: UUID!
  productId: UUID!
  qty: Int!
  variantId: UUID
  product: Product
  variant: ProductVariant
}

input CreateOrderComboLineInput {
  comboId: UUID!
  quantity: Int!
}

input CreateOrderInput {
  lines: [CreateOrderLineInput!]!
  comboLines: [CreateOrderComboLineInput!]
}
```

### Source Tree Components

| Layer | Path | Change |
|-------|------|--------|
| Domain | `src/GoiMon.Api/Domain/Entities/Order.cs` | Add `ComboId`, `ComboName` to `OrderItem`; update `AddItem()` |
| Infrastructure | `src/GoiMon.Api/Infrastructure/Data/Configurations/OrderItemConfiguration.cs` | Map new columns |
| Infrastructure | `src/GoiMon.Api/Infrastructure/Data/Migrations/` | New migration |
| API | `src/GoiMon.Api/Features/Orders/OrderMutations.cs` | Add combo line processing + new input records |
| Client Schema | `src/GoiMon.Client/schema.graphql` | Add combo fields |
| Client GQL | `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql` | Add comboLines |
| Client GQL | `src/GoiMon.Client/GraphQL/queries/GetOrders.graphql` | Add comboId, comboName |
| Client GQL | `src/GoiMon.Client/GraphQL/subscriptions/OrderSubscriptions.graphql` | Add comboId, comboName |
| Client State | `src/GoiMon.Client/State/UiCacheState.cs` | Add combo fields to snapshot |
| Client UI | `src/GoiMon.Client/Pages/Checkout.razor` | Combo tab + ordering flow |
| Client UI | `src/GoiMon.Client/Pages/Orders.razor` | Combo badge display |

### Testing Notes

- API: Test combo ordering via GraphQL playground (create combo → create order with comboLines → verify order total and item snapshots)
- Client: Verify build succeeds, combo tab renders, combo order submits correctly
- Edge cases: order with empty combo, deleted combo, combo with 0 items, mixed product+combo lines

### References

- [Source: src/GoiMon.Api/Domain/Entities/ProductCombo.cs] — Combo aggregate with Items, Price, Name
- [Source: src/GoiMon.Api/Features/Orders/OrderMutations.cs] — Current CreateOrder flow with product validation
- [Source: src/GoiMon.Api/Features/Combos/ComboMutations.cs] — Existing combo CRUD API
- [Source: src/GoiMon.Client/GraphQL/mutations/ComboMutations.graphql] — Existing client combo operations
- [Source: src/GoiMon.Client/Pages/Checkout.razor] — Current product-only checkout flow
- [Source: _bmad-output/implementation-artifacts/orders-api-feature-design.md] — Orders feature design

---

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

_(to be filled during implementation)_

### Completion Notes List

_(to be filled during implementation)_

### File List

_(to be filled during implementation)_

# Product Modifier Implementation Plan (Size + Topping)

Date: 2026-03-02  
Owner: Business Analyst (Mary) + Dev handoff

## 1) Objective

Implement configurable products where a customer can:

- Choose exactly one size (example: S/M/L)
- Choose optional add-ons (example: toppings)
- Get deterministic pricing and strict validation

Design decision: use generic **Modifier** model, not hard-coded **Topping**, so the same structure supports sugar level, ice level, milk substitution, combo choices, and future product types.

## 2) Current Baseline (from code)

- Product is currently a flat item with one `Price` in `Domain/Entities/Product.cs`.
- Order creation currently accepts client-sent snapshots (`productName`, `unitPrice`) in `Features/Orders/OrderMutations.cs`.
- Order snapshot persistence exists and should be preserved (`Domain/Entities/Order.cs`, `Infrastructure/Data/Configurations/OrderItemConfiguration.cs`).
- GraphQL schema currently has `OrderInput` + `OrderItemInput` without size/modifier structure (`src/GoiMon.Client/schema.graphql`).

## 3) Target Domain Model

### 3.1 New concepts

- `ProductVariant` (size/format choice)
  - one product has many variants
  - exactly one selected when ordering configurable products
- `ModifierGroup` (option group)
  - examples: Topping, Sugar, Ice, Milk, Side choice
  - fields: `Name`, `SelectionMode` (Single/Multi), `MinSelect`, `MaxSelect`, `SortOrder`, `IsRequired`
- `ModifierOption`
  - fields: `Name`, `PriceDelta`, `IsDefault`, `MaxQty`, `SortOrder`, `IsActive`
- `VariantModifierRule` (optional bridge)
  - defines whether an option is allowed for a specific variant

### 3.2 Order snapshot extension

Extend order snapshots to preserve historical detail:

- line-level: selected variant name + variant unit price
- child-level: selected modifier options + quantity + option price delta
- keep existing `ProductName` + `UnitPrice` semantics for backward compatibility

## 4) Execution Plan

## Phase 1 — API Data Model & Persistence (MVP foundation)

### Tasks

1. Add new entities under feature/domain structure:
   - `src/GoiMon.Api/Domain/Entities/ProductVariant.cs`
   - `src/GoiMon.Api/Domain/Entities/ModifierGroup.cs`
   - `src/GoiMon.Api/Domain/Entities/ModifierOption.cs`
   - `src/GoiMon.Api/Domain/Entities/OrderItemModifier.cs`
2. Register new `DbSet<>` in `src/GoiMon.Api/Infrastructure/Data/AppDbContext.cs`.
3. Add EF configurations in `src/GoiMon.Api/Infrastructure/Data/Configurations/`:
   - `ProductVariantConfiguration.cs`
   - `ModifierGroupConfiguration.cs`
   - `ModifierOptionConfiguration.cs`
   - `OrderItemModifierConfiguration.cs`
4. Create and apply EF migration.
5. Seed basic demo data: one milk tea product with S/M/L and topping group.

### Acceptance criteria

- Database stores variants and modifier catalog linked to products.
- Existing products/orders continue to work without migration break.
- Existing GraphQL queries still run unchanged.

## Phase 2 — GraphQL Contract + Order Command Validation

### Tasks

1. Add GraphQL types/inputs under `src/GoiMon.Api/Features/Products/` and `src/GoiMon.Api/Features/Orders/`:
   - `ProductVariantType`, `ModifierGroupType`, `ModifierOptionType`
   - `CreateOrderLineInput` (replacing/augmenting current `OrderItemInput`)
   - `OrderLineModifierInput`
2. Update `OrderMutations.CreateOrder` flow:
   - accept `productId`, `variantId`, `quantity`, `modifiers[]`
   - server resolves active product/variant/modifier data
   - server computes authoritative price
3. Add validators:
   - selected variant belongs to product and is active
   - selected options belong to allowed groups and satisfy min/max rules
   - option quantity within `MaxQty`
4. Return structured error codes (`INVALID_VARIANT`, `GROUP_MAX_EXCEEDED`, `OPTION_NOT_ALLOWED`).

Decision update (2026-03-02):

- Backward compatibility for legacy `OrderInput` is intentionally removed for this phase.
- `createOrder` now accepts only the new configurable order contract.

### Acceptance criteria

- Invalid combinations are rejected server-side.
- `unitPrice` and `lineTotal` are computed by API, not trusted from client.
- Existing order snapshot compatibility maintained.

## Phase 3 — Client GraphQL + UI Flow (BlazorBlueprint only)

### Tasks

1. Add GraphQL operations:
   - `src/GoiMon.Client/GraphQL/Products/GetProductConfigurator.graphql`
   - update `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`
2. Regenerate StrawberryShake client from `.graphql` operations.
3. Add feature-scoped client state:
   - `src/GoiMon.Client/Features/Orders/State/ProductConfiguratorState.cs`
4. Implement configurator UI using BlazorBlueprint components only:
   - variant picker (single choice)
   - modifier groups (single/multi + qty)
   - real-time price summary
5. Update order submit flow to send variant/modifier inputs only.

### Acceptance criteria

- User can configure S/M/L + toppings in one order line.
- UI enforces min/max rules and disables unavailable options.
- Submitted order total matches API-calculated total.

## Phase 4 — Production Rollout & Telemetry

### Tasks

1. Apply atomic deployment strategy (API + Client together) because contract is breaking.
2. Add telemetry:
   - count invalid configuration attempts
   - count orders using new modifier path
3. Remove deprecated input path and cleanup legacy validators/contracts.

### Acceptance criteria

- Atomic API + Client deployment is documented and repeatable.
- Runtime counters/logs are available for validation failures and successful configurable orders.

## 5) Proposed GraphQL Shape (target)

```graphql
input CreateOrderLineInput {
  productId: UUID!
  variantId: UUID
  quantity: Int!
  modifiers: [OrderLineModifierInput!]
}

input OrderLineModifierInput {
  optionId: UUID!
  quantity: Int!
}
```

Notes:

- `variantId` may be nullable only for non-configurable products.
- API should always return resolved snapshot text for receipts.

## 6) Risks & Mitigations

- Risk: price mismatch between client and server  
  Mitigation: API is source of truth; client only displays estimate.
- Risk: schema break for existing clients  
  Mitigation: dual-input deprecation window.
- Risk: complex validation logic spread across resolvers  
  Mitigation: centralize in validators + domain service.

## 7) Implementation Checklist (ready to execute)

- [x] Phase 1 entities + EF configurations + migration
- [x] Phase 2 GraphQL input/type changes + server pricing/validation (API)
- [x] Phase 3 client operations + StrawberryShake regen
- [x] Phase 3 configurator UI/checkout page
- [x] Phase 4 rollout + telemetry + cleanup

## 7.1 Current execution status

- Completed: domain entities, EF configurations, `DbSet` registrations, and migration `AddProductModifiers`.
- Completed: seed sample beverage data (`Milk Tea`) with S/M/L variants and topping options.
- Phase 1 status: done.
- Completed: API mutation `createConfiguredOrder` with server-side product/variant/modifier validation and authoritative pricing.
- Completed: legacy order input contract removed; production `createOrder` now uses only configurable lines + modifiers.
- Completed: client GraphQL operations updated for variants/modifierGroups and new `createOrder` payload.
- Completed: StrawberryShake schema update + client code generation.
- Completed: Orders UI now reads and displays selected modifier snapshots in order details.
- Completed: new checkout page for product configuration and order creation (`/checkout`) integrated into sidebar navigation.
- Completed: API telemetry counters/logging for configurable order creation and validation failures.
- Completed: Phase 4 production rollout notes documented in `product-modifier-phase4-rollout.md`.

## 8) First Sprint Scope (recommended)

Focus only on:

- Beverage products
- One required variant group (`Size`)
- One optional multi-select group (`Topping`)
- No nested/bundle modifiers in sprint 1

This keeps MVP small while preserving a generic model for future expansion.
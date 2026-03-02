# Product Modifier Phase 4 Rollout (Production)

Date: 2026-03-02

## Scope

- Contract is **breaking** by design.
- Legacy `OrderInput` path is removed.
- All clients must use `CreateOrderInput` with `lines` and `modifiers`.

## Deployment Strategy

1. Deploy API and Client in one release window.
2. Invalidate old static client cache (if CDN/browser cache policy keeps old app bundle).
3. Run smoke check:
   - query products with `variants` and `modifierGroups`
   - create order with valid size + topping
   - create order with invalid input and verify error payload

## Runtime Telemetry

The API emits runtime telemetry for configurable order flow:

- `goimon.orders.config.validation_failed`
  - increments when create order payload fails business validation
  - tags: `line_count`, `error_count`
- `goimon.orders.config.created`
  - increments when a configurable order is successfully created
  - tags: `line_count`
- `goimon.orders.config.selected_modifiers`
  - sums selected modifier quantities for created orders
  - tags: `line_count`

Log entries are also emitted with structured values for validation failures and successful order creation.

## Cleanup Status

- Legacy order input contract: removed.
- Legacy validator for old order input: removed.
- Client operations regenerated against new schema.

## Rollback Note

Because the schema contract is breaking, rollback requires rolling back both API and Client together.

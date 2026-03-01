# Orders API Feature Design (GoiMon.Api)

Date: 2026-03-02  
Owner: Developer Agent (Amelia)

## 1) Current State (as-is)

- Existing files:
  - `src/GoiMon.Api/Features/Orders/OrderMutations.cs` (only `CreateOrder` mutation)
  - `src/GoiMon.Api/Features/Orders/OrderQueries.cs` (fully commented out)
  - `src/GoiMon.Api/Features/Orders/Validators/OrderInputValidator.cs`
- `Order` aggregate already supports:
  - add/remove line items
  - status transitions (`Open`, `Completed`, `Paid`, `Cancelled` enum exists)
  - domain events (`OrderCreatedEvent`, `OrderItemAddedEvent`, `OrderCompletedEvent`)
- Infrastructure already supports:
  - EF Core mappings (`OrderConfiguration`, `OrderItemConfiguration`)
  - GraphQL server setup with filtering/projection/sorting middleware
  - Domain-event-to-outbox persistence in `AppDbContext.SaveChangesAsync`

## 2) Design Goals

1. Complete the Orders GraphQL API with production-ready query + mutation coverage.
2. Keep feature self-contained under `Features/Orders` (feature-folder convention).
3. Enforce validation with FluentValidation at input boundary.
4. Keep domain invariants in aggregate methods (not in resolver logic).
5. Support operational workflows: create → update items → complete/pay/cancel.

## 3) Target Feature Structure

```text
src/GoiMon.Api/Features/Orders/
  Models/
    OrderItemDto.cs
    OrderPricingSummary.cs
  Inputs/
    CreateOrderInput.cs
    UpdateOrderInput.cs
    AddOrderItemInput.cs
    UpdateOrderItemInput.cs
  Mutations/
    OrderMutations.cs
  Queries/
    OrderQueries.cs
  Types/
    OrderType.cs
    OrderItemType.cs
  Services/
    IOrderPricingService.cs
    OrderPricingService.cs
  Validators/
    CreateOrderInputValidator.cs
    UpdateOrderInputValidator.cs
    AddOrderItemInputValidator.cs
```

Notes:
- Split by responsibility (Mutations/Queries/Inputs/Validators) to align with Authentication feature style.
- Keep domain entities in `src/GoiMon.Api/Domain/Entities` unchanged as aggregate source of truth.

## 4) GraphQL Contract (proposed)

### Queries

1. `orders` (offset paging + projection + filtering + sorting)
   - Returns `IQueryable<Order>` with `Include(o => o.Items)` only when needed.
2. `orderById(id: UUID!)`
   - Returns one order with items.
3. `ordersByStatus(status: OrderStatus!)`
   - Fast status board query for kitchen/cashier screens.
4. `openOrders()`
   - Shortcut query for active workflows.

### Mutations

1. `createOrder(input: CreateOrderInput!) : Order!`
2. `addOrderItem(input: AddOrderItemInput!) : Order!`
3. `updateOrderItem(input: UpdateOrderItemInput!) : Order!`
4. `removeOrderItem(orderId: UUID!, orderItemId: UUID!) : Order!`
5. `completeOrder(orderId: UUID!) : Order!`
6. `markOrderPaid(orderId: UUID!) : Order!`
7. `cancelOrder(orderId: UUID!, reason: String) : Order!`

Design decision:
- Return updated `Order` from mutations (not `bool`) to reduce round-trip for client refresh.

## 5) Input Models

### `CreateOrderInput`
- `items: [CreateOrderItemInput!]!`

### `CreateOrderItemInput`
- `productId: UUID!`
- `qty: Int!` (must be > 0)
- `unitPrice: Decimal!` (must be >= 0)

### `AddOrderItemInput`
- `orderId: UUID!`
- `productId: UUID!`
- `qty: Int!`
- `unitPrice: Decimal!`

### `UpdateOrderItemInput`
- `orderId: UUID!`
- `orderItemId: UUID!`
- `qty: Int!`
- `unitPrice: Decimal!`

### `UpdateOrderInput`
- reserved for future metadata (note/customer/table/tags), keep optional in phase 2.

## 6) Validation Rules

Apply FluentValidation for all input objects:

- Order creation:
  - at least 1 item
  - no duplicate `productId` in the same payload (merge responsibility explicit)
- Item rules:
  - `productId` not empty
  - `qty > 0`
  - `unitPrice >= 0`
- Transition rules:
  - cannot complete/cancel/paid when order not found
  - cannot add/update/remove items once status is `Completed` or `Paid`
  - cannot mark paid if status is `Cancelled`

## 7) Domain & Service Responsibilities

### Domain (`Order` aggregate)
- Owns invariants for items and status changes.
- Add methods if needed:
  - `MarkPaid()`
  - `Cancel()`
  - `UpdateItem(orderItemId, qty, unitPrice)`
- Raise domain events for state transitions:
  - `OrderPaidEvent`
  - `OrderCancelledEvent`
  - `OrderItemUpdatedEvent`

### Application Service (`IOrderPricingService`)
- Optional phase-2 service to centralize pricing logic:
  - subtotal, discounts, taxes, final total
- Keeps resolver thin when pricing rules expand.

## 8) Data Access Pattern

- Keep GraphQL DB-backed pattern used by `ProductQueries`:
  - `[UseDbContext(typeof(AppDbContext))]`
  - `[UseOffsetPaging(IncludeTotalCount = true)]`
  - `[UseProjection]`
  - `[UseFiltering]`
  - `[UseSorting]`
- Use pooled context injection in resolvers:
  - `[Service(ServiceKind.Pooled)] AppDbContext db`
- Include order items when command/query requires full aggregate behavior.

## 9) Program.cs Registration Changes

Ensure both extensions are registered:

- `.AddTypeExtension<OrderQueries>()`
- `.AddTypeExtension<OrderMutations>()`

Current gap: only `OrderMutations` is registered; `OrderQueries` is not active.

## 10) Testing Strategy

Location:
- `tests/GoiMon.Api.Tests/Features/Orders/`

Tests:
1. Query tests
   - paging/filter/sort for `orders`
   - `orderById` returns null when missing
2. Mutation tests
   - create with valid payload
   - reject invalid qty/unitPrice
   - add/update/remove item recalculates total correctly
   - complete/paid/cancel transition behavior
3. Validation tests
   - each validator positive + negative cases
4. Domain tests
   - `Order` aggregate invariants and event raising

## 11) Delivery Phases

### Phase 1 (MVP)
- Activate `OrderQueries`
- Refactor Orders folder into `Mutations`, `Queries`, `Inputs`, `Validators`
- Implement create/add/remove/complete/cancel
- Add tests for resolver + validator + aggregate core paths

### Phase 2
- add paid transition + update item
- add `ordersByStatus`, `openOrders`
- add pricing service and dto projections

### Phase 3
- enrich events/outbox consumers for kitchen/notification integrations
- add auth/policy constraints by role (cashier, manager, kitchen)

## 12) Non-Goals (for this design)

- Payment gateway integration
- Inventory reservation/stock deduction
- Kitchen printing integration
- Client-side StrawberryShake operations (separate client story)

---

## Implementation Readiness Checklist

- [ ] Orders feature folder refactored to target structure
- [ ] Queries activated and registered in GraphQL
- [ ] Input/validator split completed
- [ ] Status transition methods complete in aggregate
- [ ] Resolver and domain tests passing
- [ ] Migration required? (only if entity shape changes)

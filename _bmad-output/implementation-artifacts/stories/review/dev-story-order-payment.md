# 💳 Dev Story: Mark Order as Paid (Cashier Flow)

**Status:** In Review  
**Date Created:** 2026-03-03  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 3-2-order-payment

---

## Story

As a **cashier/staff**,  
I want to **mark a completed order as paid**,  
so that **the store can distinguish served-but-unpaid orders from fully settled orders**.

---

## Scope Decision (MVP)

Use existing `OrderStatus.Paid` in current domain model (no new `PaymentStatus` table/enum yet).

- Current statuses: `Open`, `Completed`, `Paid`, `Cancelled`
- New flow: `Open -> Completed -> Paid`
- `Paid` means: operationally completed + payment confirmed

This avoids schema explosion now and solves the immediate cashier UX gap.

---

## Acceptance Criteria

- [x] **AC1**: API exposes mutation `markOrderPaid(orderId: UUID!): Order`  
- [x] **AC2**: Mutation rule: only orders in `Completed` status can be marked paid  
- [x] **AC3**: Mutation rejects invalid transitions (`Open -> Paid`, `Cancelled -> Paid`) with clear error message  
- [x] **AC4**: Orders UI shows a visible payment marker (badge/state) for paid orders  
- [x] **AC5**: Orders detail panel has action button `Đánh dấu đã thanh toán` when status is `Completed`  
- [x] **AC6**: After marking paid, order list and detail refresh immediately (including realtime subscription path)  
- [x] **AC7**: Tabs/counts correctly include paid orders (current completed bucket may remain merged in MVP)  
- [x] **AC8**: Build succeeds with zero compile errors for API + Client

---

## Task Breakdown

### TASK 1 — Domain: Add pay transition on aggregate (AC: #2, #3)

- [x] Add method `MarkPaid()` on `Order` aggregate
- [x] Guard clause: allow only when `Status == OrderStatus.Completed`
- [x] Set `Status = OrderStatus.Paid`
- [x] Raise domain event `OrderPaidEvent` (optional but recommended for telemetry/outbox consistency)

**Files:**
- `src/GoiMon.Api/Domain/Entities/Order.cs`
- `src/GoiMon.Api/Domain/Events/Events.cs` (if adding new event)

---

### TASK 2 — API: GraphQL mutation (AC: #1, #2, #3)

- [x] Add mutation method in `OrderMutations`:
  - `public async Task<Order?> MarkOrderPaid(Guid orderId, ...)`
- [x] Load order + validate existence
- [x] Execute `order.MarkPaid()` and persist
- [x] Publish order-changed topic for client refresh
- [x] Return updated order

**Files:**
- `src/GoiMon.Api/Features/Orders/OrderMutations.cs`

---

### TASK 3 — Client GraphQL operations (AC: #1, #6)

- [x] Add mutation operation:
  - `mutation MarkOrderPaid($orderId: UUID!) { markOrderPaid(orderId: $orderId) { id status } }`
- [x] Ensure query/subscription already includes `status` (if yes, keep)
- [x] Regenerate StrawberryShake client via build

**Files:**
- `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`
- Generated client artifacts (auto)

---

### TASK 4 — Orders UI action + marker (AC: #4, #5, #6, #7)

- [x] In Orders detail actions:
  - show button `Đánh dấu đã thanh toán` only when selected order status is `COMPLETED`
- [x] On click:
  - call `Client.MarkOrderPaid.ExecuteAsync(orderId)`
  - show success/error toast
  - refresh list state (or rely on subscription + local patch)
- [x] In list item and detail header:
  - show badge for `PAID` state (e.g. `Đã thanh toán`)
- [x] Keep MVP tab behavior:
  - `completed` bucket includes both `COMPLETED` and `PAID` (matches current code path)

**Files:**
- `src/GoiMon.Client/Pages/Orders.razor`
- `src/GoiMon.Client/Features/Orders/Components/*` (if action toolbar component exists)

---

## UX Notes

- Button text: `Đánh dấu đã thanh toán`
- Optional confirmation dialog for cashier safety: `Xác nhận đã thu tiền cho đơn này?`
- On success: disable/hide button immediately to prevent double-submit

---

## Non-Goals (Phase 2)

- Separate `PaymentStatus` dimension (`Unpaid`, `PartiallyPaid`, `Paid`, `Refunded`)
- Payment method tracking (cash/bank transfer/QR/card)
- Partial payment / split bills / refund workflows

---

## References (Current Code Signals)

- `OrderStatus` already contains `Paid`: `src/GoiMon.Api/Domain/Entities/OrderStatus.cs`
- Orders UI currently groups paid into completed tab path:
  - status filter includes `OrderStatus.Paid`
  - tab mapping includes `"completed" => normalizedStatus is "COMPLETED" or "PAID"`

---

## Dev Agent Record

### Agent Model Used

GPT-5.3-Codex

### Completion Notes List

- Domain transition `Completed -> Paid` implemented with `Order.MarkPaid()` and invalid-transition guard.
- API mutation `markOrderPaid(orderId)` is implemented and publishes order-changed event for realtime updates.
- Client mutation call, paid status marker, and paid tab/count behavior are implemented in Orders UI.
- Build verification completed successfully (`dotnet build GoiMon.sln`).

### File List

- `src/GoiMon.Api/Domain/Entities/Order.cs`
- `src/GoiMon.Api/Domain/Events/Events.cs`
- `src/GoiMon.Api/Features/Orders/OrderMutations.cs`
- `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`
- `src/GoiMon.Client/Pages/Orders.razor`
- `src/GoiMon.Client/Features/Orders/Components/OrderDetailPanel.razor`
- `src/GoiMon.Client/schema.graphql`
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-order-payment.md`

### Change Log

- 2026-03-05 — Story status moved `Ready -> In Review`; acceptance criteria and task checklist updated based on implemented code and successful solution build.

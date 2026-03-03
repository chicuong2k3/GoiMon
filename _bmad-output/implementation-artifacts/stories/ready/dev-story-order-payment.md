# 💳 Dev Story: Mark Order as Paid (Cashier Flow)

**Status:** Ready  
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

- [ ] **AC1**: API exposes mutation `markOrderPaid(orderId: UUID!): Order`  
- [ ] **AC2**: Mutation rule: only orders in `Completed` status can be marked paid  
- [ ] **AC3**: Mutation rejects invalid transitions (`Open -> Paid`, `Cancelled -> Paid`) with clear error message  
- [ ] **AC4**: Orders UI shows a visible payment marker (badge/state) for paid orders  
- [ ] **AC5**: Orders detail panel has action button `Đánh dấu đã thanh toán` when status is `Completed`  
- [ ] **AC6**: After marking paid, order list and detail refresh immediately (including realtime subscription path)  
- [ ] **AC7**: Tabs/counts correctly include paid orders (current completed bucket may remain merged in MVP)  
- [ ] **AC8**: Build succeeds with zero compile errors for API + Client

---

## Task Breakdown

### TASK 1 — Domain: Add pay transition on aggregate (AC: #2, #3)

- Add method `MarkPaid()` on `Order` aggregate
- Guard clause: allow only when `Status == OrderStatus.Completed`
- Set `Status = OrderStatus.Paid`
- Raise domain event `OrderPaidEvent` (optional but recommended for telemetry/outbox consistency)

**Files:**
- `src/GoiMon.Api/Domain/Entities/Order.cs`
- `src/GoiMon.Api/Domain/Events/Events.cs` (if adding new event)

---

### TASK 2 — API: GraphQL mutation (AC: #1, #2, #3)

- Add mutation method in `OrderMutations`:
  - `public async Task<Order?> MarkOrderPaid(Guid orderId, ...)`
- Load order + validate existence
- Execute `order.MarkPaid()` and persist
- Publish order-changed topic for client refresh
- Return updated order

**Files:**
- `src/GoiMon.Api/Features/Orders/OrderMutations.cs`

---

### TASK 3 — Client GraphQL operations (AC: #1, #6)

- Add mutation operation:
  - `mutation MarkOrderPaid($orderId: UUID!) { markOrderPaid(orderId: $orderId) { id status } }`
- Ensure query/subscription already includes `status` (if yes, keep)
- Regenerate StrawberryShake client via build

**Files:**
- `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`
- Generated client artifacts (auto)

---

### TASK 4 — Orders UI action + marker (AC: #4, #5, #6, #7)

- In Orders detail actions:
  - show button `Đánh dấu đã thanh toán` only when selected order status is `COMPLETED`
- On click:
  - call `Client.MarkOrderPaid.ExecuteAsync(orderId)`
  - show success/error toast
  - refresh list state (or rely on subscription + local patch)
- In list item and detail header:
  - show badge for `PAID` state (e.g. `Đã thanh toán`)
- Keep MVP tab behavior:
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

- Story drafted from current domain and UI behavior to minimize refactor risk.

### File List

- `_bmad-output/implementation-artifacts/dev-story-order-payment.md`

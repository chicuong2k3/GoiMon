# 🍽️ Dev Story: Table Management Core (Virtual Slots + Service/Payment Tracking)

**Status:** Ready  
**Date Created:** 2026-03-05  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 3-3-table-management-core

---

## Story

As a **staff member**,  
I want to **manage virtual table slots and track each table's kitchen/service/payment state**,  
so that **I can know which table needs food delivery and which table is ready for payment/closure**.

---

## Scope

### In Scope
- `TableSlot` management (list/create/update/deactivate)
- Optional table assignment on order (`Order.TableSlotId`, nullable for takeaway)
- One active order per table slot enforcement
- Service/payment state progression in dine-in workflow:
  - `Available`
  - `Occupied`
  - `Preparing`
  - `ReadyToServe`
  - `AwaitingPayment`
  - `Paid`
- Table board screen with status counters and quick actions
- Operational actions from active order context:
  - split bill
  - merge table slots
  - split table slot
- Realtime synchronization for table/order status changes

### Out of Scope
- Reservation management
- Visual floor-map designer
- Advanced seat optimization
- Multi-store table orchestration
- Table analytics dashboard (post-MVP)

---

## Dependencies

- Existing order lifecycle baseline (create/complete/cancel/subscription)
- Existing paid flow story (`3-2-order-payment`) for final payment closure consistency
- GraphQL + StrawberryShake generation workflow
- Client state baseline with `StoreComponentWithUtilities<TState>` and domain-scoped store pattern
- Product direction from brainstorming artifact:
  - `_bmad-output/brainstorming/brainstorming-session-2026-03-05.md`

---

## Acceptance Criteria

- [ ] **AC1**: System supports `TableSlot` list/create/update/deactivate operations.
- [ ] **AC2**: Order can be created/updated with optional `TableSlotId` (`null` for takeaway).
- [ ] **AC3**: Dine-in orders enforce one active order per table slot.
- [ ] **AC4**: Table/order states support kitchen-progress and payment milestones (`Preparing`, `ReadyToServe`, `AwaitingPayment`, `Paid`).
- [ ] **AC5**: UI provides table board view showing status counts and slot-level quick actions.
- [ ] **AC6**: Staff can execute split bill, merge table, and split table from active order context.
- [ ] **AC7**: Realtime updates reflect status changes without manual refresh.
- [ ] **AC8**: API + Client builds pass and end-to-end mixed dine-in/takeaway flow is manually verified.

---

## Task Breakdown

### TASK 1 — Domain + Persistence (AC: #1, #2, #3, #4)
- [ ] Add `TableSlot` aggregate/entity and persistence mapping.
- [ ] Add nullable `TableSlotId` relation on order.
- [ ] Add invariant enforcement for one active order per slot.
- [ ] Add migration(s) and backfill-safe defaults.

**Files:**
- `src/GoiMon.Api/Domain/Entities/TableSlot.cs`
- `src/GoiMon.Api/Domain/Entities/Order.cs`
- `src/GoiMon.Api/Infrastructure/Persistence/*`
- `src/GoiMon.Api/Infrastructure/Persistence/Migrations/*`

### TASK 2 — API GraphQL: Tables Feature (AC: #1, #2, #3, #4, #6)
- [ ] Create `Features/Tables` with queries/mutations/types.
- [ ] Implement service transitions and action endpoints (merge/split/split-bill orchestration).
- [ ] Add validators and clear error semantics for slot conflicts.

**Files:**
- `src/GoiMon.Api/Features/Tables/Queries/*`
- `src/GoiMon.Api/Features/Tables/Mutations/*`
- `src/GoiMon.Api/Features/Tables/Models/*`
- `src/GoiMon.Api/Features/Tables/Validators/*`

### TASK 3 — Client GraphQL + State (AC: #1, #2, #4, #7)
- [ ] Add `.graphql` operations for table slots and service actions.
- [ ] Generate StrawberryShake client contracts for tables feature.
- [ ] Add `TablesUiState` store and integrate with orders state sync.

**Files:**
- `src/GoiMon.Client/GraphQL/Tables/*.graphql`
- `src/GoiMon.Client/State/TablesUiState.cs`
- `src/GoiMon.Client/Program.cs` (store registration)

### TASK 4 — Client UI: Table Board + Flow Integration (AC: #2, #5, #6, #7)
- [ ] Add `Tables` page with board/status counters/filters.
- [ ] Add slot selector for dine-in in checkout flow.
- [ ] Add quick status/actions in order detail context.

**Files:**
- `src/GoiMon.Client/Pages/Tables.razor`
- `src/GoiMon.Client/Pages/Checkout.razor`
- `src/GoiMon.Client/Pages/Orders.razor`
- `src/GoiMon.Client/Features/Orders/Components/OrderDetailPanel.razor`

### TASK 5 — Verification + QA Flow (AC: #8)
- [ ] Validate mixed operation mode (dine-in + takeaway concurrently).
- [ ] Validate conflict handling (slot already occupied).
- [ ] Validate transition consistency and realtime reflection across screens.

**Files:**
- `tests/GoiMon.Api.Tests/Features/Tables/*`
- `tests/GoiMon.Client.Tests/Features/Tables/*` (if matching current test style)

---

## Verification Plan

- [ ] Build command(s):
  - `dotnet build src/GoiMon.Api/GoiMon.Api.csproj`
  - `dotnet build src/GoiMon.Client/GoiMon.Client.csproj`
- [ ] Manual scenario(s):
  - Create takeaway order (no table) and dine-in order (with table) in parallel.
  - Move dine-in through `Preparing -> ReadyToServe -> AwaitingPayment -> Paid`.
  - Confirm payment releases slot to `Available`.
  - Verify split-bill + merge/split table actions from active order context.
- [ ] Edge case(s):
  - Prevent two active orders attaching to the same slot.
  - Handle deactivated slot when an old order still references it.
  - Recover gracefully when realtime subscription events arrive out of order.

---

## Definition of Ready (DoR)

- [x] Story key is unique
- [x] Scope in/out is explicit
- [x] Acceptance criteria are measurable
- [x] Dependencies are listed
- [x] Impacted files/layers are identified

---

## Definition of Done (DoD)

- [ ] All ACs completed
- [ ] Build passes on impacted projects
- [ ] Manual validation completed
- [ ] Story status/folder updated to match state
- [ ] Story board row updated

---

## Dev Notes

### Design Decisions
1. Treat table as an operational slot (`TableSlot`), not strictly a physical furniture object.
2. Keep one-active-order-per-slot invariant to reduce service ambiguity.
3. Include kitchen-progress in MVP (confirmed in brainstorming), not deferred.
4. Preserve mixed operation model with optional table assignment (`null` means takeaway).

### Risks
- Merge/split/split-bill can introduce order-total reconciliation complexity if sequencing is unclear.
- Realtime and offline synchronization may show temporary status drift without strict transition guards.

### Product Decisions Captured
- Business mode: mixed dine-in + takeaway.
- Virtualized slot model is required.
- Kitchen-progress states are required in MVP.

---

## Change Log

- 2026-03-05 — Story created in `ready/` from finalized brainstorming blueprint by Mary (Business Analyst)

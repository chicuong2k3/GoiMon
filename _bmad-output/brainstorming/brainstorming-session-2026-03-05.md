---
stepsCompleted: [1, 2, 3]
inputDocuments: []
session_topic: 'Add Table Management (numbered tables + service/payment tracking) to GoiMon'
session_goals: 'Produce a full feature blueprint: business rationale, user workflows, MVP scope, acceptance criteria, and implementation-ready story draft'
selected_approach: 'ai-recommended'
techniques_used:
	- 'Question Storming'
	- 'SCAMPER Method'
	- 'Constraint Mapping'
ideas_generated:
	- 'Virtual table slot model'
	- 'One-open-order-per-slot'
	- 'Kitchen-progress in MVP'
	- 'Table merge/split operations'
	- 'Mixed dine-in/takeaway support'
context_file: '_bmad/bmm/data/project-context-template.md'
---

# Brainstorming Session Results

**Facilitator:** Chicuong
**Date:** 2026-03-05

## Session Overview

**Topic:** Add Table Management (numbered tables + service/payment tracking) to GoiMon
**Goals:** Produce a full feature blueprint: business rationale, user workflows, MVP scope, acceptance criteria, and implementation-ready story draft

### Context Guidance

- Focus on user pain points, feature capabilities, UX flow, business value, and technical risks.
- Align with GoiMon's current direction: simple, offline-first, low-friction operations for small merchants.

### Session Setup

- Outcome mode selected: **C — Full feature blueprint**.
- Assumption for this session: prioritize operational clarity (table status, payment visibility, service handoff) while preserving MVP simplicity.

## Technique Selection

**Approach:** AI-Recommended Techniques  
**Analysis Context:** Add Table Management (numbered tables + service/payment tracking) with focus on full blueprint output

**Recommended Techniques:**

- **Question Storming:** Clarify operational rules and edge cases before solutioning
- **SCAMPER Method:** Generate broad feature options and interaction variants quickly
- **Constraint Mapping:** Converge on realistic MVP and rollout phases under offline-first and simplicity constraints

**AI Rationale:**

This sequence fits GoiMon's context: first reduce ambiguity (what problem to solve), then expand options (what we could build), then narrow by constraints (what we should ship now). It balances creativity with implementation realism for a directly actionable blueprint.

## Technique Execution Results

### Technique 1 — Question Storming (Completed)

**Captured Inputs (User):**
- Operation model: **Mixed dine-in + takeaway**
- Main pain point: **service/delivery tracking**
- Order model: **1 open order per virtual table**
- Table concept: **virtualized table slots** (example: 1 physical table with 2 seats can be represented as 2 table codes)
- Advanced operations in scope: **split bill = yes, merge table = yes, split table = yes**

**Key Clarifications Derived:**
- `Table` should be treated as an **operational slot**, not strictly a physical furniture entity.
- One-order-per-slot simplifies current flow and prevents accidental multi-order conflicts on the same slot.
- Service orchestration is a first-class business need (not only checkout/reconciliation).

**Interim Product Direction:**
- Add `TableSlot` management with lightweight lifecycle and occupancy state.
- Keep order as primary transaction object, but attach optional `TableSlotId`.
- Prioritize visibility states that directly help service handoff and payment tracking.

**Decision Locked (User Confirmed):**
- MVP **must include kitchen-progress states** (not deferred).

### Technique 2 — SCAMPER Method (Completed)

**S — Substitute**
- Substitute physical-table-only model with **virtual table slots** (`A1`, `A2`, `B1`), allowing 1 physical table to map to many serving positions.
- Substitute manual waiter memory with **state badges** and color-coded service board.

**C — Combine**
- Combine order lifecycle + table lifecycle into one operational flow (`Occupied -> Preparing -> ReadyToServe -> AwaitingPayment -> Paid`).
- Combine dine-in and takeaway in one queue with optional `TableSlotId` (`null` for takeaway).

**A — Adapt**
- Adapt current order tabs into service-oriented views: `Cần làm`, `Sẵn sàng mang`, `Chờ thanh toán`.
- Adapt existing paid-flow to table context: payment closes slot occupancy.

**M — Modify**
- Modify checkout to optionally require/select a `TableSlot` for dine-in.
- Modify order detail to expose quick actions: `Mark Preparing`, `Mark Ready`, `Mark Served`, `Mark Paid`.

**P — Put to Other Use**
- Use table status board as shift handoff dashboard.
- Use historical table occupancy for peak-hour staffing insight (post-MVP analytics).

**E — Eliminate**
- Eliminate ambiguous “who ordered this” by forcing one active ticket per slot.
- Eliminate duplicate serving actions with explicit served/ready transitions.

**R — Reverse**
- Reverse from payment-first to service-first process; payment status is finalization, not preparation trigger.
- Reverse table assignment timing: allow assign-at-checkout or assign-after-create (for flexible floor operation).

### Technique 3 — Constraint Mapping (Completed)

**Core Constraints**
- Must remain simple for small merchants and fast for peak-hour staff usage.
- Must preserve offline-first behavior and local write reliability.
- Must integrate with existing order status model without heavy schema disruption.

**Design Constraints Applied**
- Keep role model simple (`Owner/Staff`) and operational actions button-driven.
- Reuse existing order object; add minimal relation to `TableSlot`.
- Keep table model lightweight: no reservations, no floor-plan designer in MVP.

**Technical Constraints Applied**
- Server: add `TableSlot` entity + small set of mutations/queries.
- Client: add table board view + slot selector in checkout + order detail quick actions.
- Realtime: extend current order subscription mapping to reflect table status transitions.

---

## Final Blueprint — Table Management Feature

### 1) Recommendation

**Yes — add table management now in MVP** for mixed dine-in + takeaway operations, because it directly resolves service tracking pain and payment visibility with low-to-medium implementation risk.

### 2) MVP Functional Scope

- `TableSlot` CRUD (create/edit/deactivate basic slots)
- Assign order to optional table slot (`null` for takeaway)
- One active order per slot (enforced)
- Table status model with kitchen progress:
	- `Available`
	- `Occupied`
	- `Preparing`
	- `ReadyToServe`
	- `AwaitingPayment`
	- `Paid`
- Actions:
	- merge table slots
	- split table slots
	- split bill
- Table board screen with quick filters/status counts

### 3) Data Model Direction

- Add `TableSlot` aggregate:
	- `Id`, `Code`, `Area`, `Capacity?`, `IsActive`, timestamps
- Add optional relation from order:
	- `Order.TableSlotId` (nullable for takeaway)
- Add service state transition API on order/table workflow.

### 4) UX Flow (MVP)

1. Staff creates/opens order.
2. If dine-in: selects `TableSlot` (virtual slot code).
3. Kitchen/service marks preparing and ready states.
4. Staff serves and marks payment.
5. On payment success: order finalizes and slot returns `Available`.

### 5) Non-Goals (Post-MVP)

- Visual floor map designer
- Reservation management
- Advanced seat optimization
- Multi-store table orchestration

---

## Implementation-Ready Story Draft

### Story Key

`3-3-table-management-core`

### Story

As a **staff member**,
I want to **manage virtual table slots and track each table's kitchen/service/payment state**,
so that **I can know which table needs food delivery and which table is ready for payment/closure**.

### Acceptance Criteria

- **AC1**: System supports `TableSlot` list/create/update/deactivate operations.
- **AC2**: Order can be created/updated with optional `TableSlotId`.
- **AC3**: Dine-in orders enforce one active order per table slot.
- **AC4**: Table/order states support kitchen-progress and payment milestones (`Preparing`, `ReadyToServe`, `AwaitingPayment`, `Paid`).
- **AC5**: UI provides table board view showing status counts and slot-level quick actions.
- **AC6**: Staff can execute split bill, merge table, split table from active order context.
- **AC7**: Realtime updates reflect status changes without manual refresh.
- **AC8**: API + Client builds pass and end-to-end dine-in service flow is manually verified.

### Task Slices

1. **Domain/API:** add `TableSlot` entity + GraphQL queries/mutations + status transitions.
2. **Client/State:** add `TableSlot` store state + GraphQL operations + sync with orders.
3. **UI:** add `Tables` page/board + slot selector in checkout + actions in order detail.
4. **Validation:** flow test for dine-in and takeaway coexistence.

### Suggested File Targets

- `src/GoiMon.Api/Features/Tables/*`
- `src/GoiMon.Api/Domain/Entities/TableSlot.cs`
- `src/GoiMon.Client/Pages/Tables.razor`
- `src/GoiMon.Client/GraphQL/Tables/*.graphql`
- `src/GoiMon.Client/State/TablesUiState.cs`
- `src/GoiMon.Client/Pages/Checkout.razor` (slot selection)
- `src/GoiMon.Client/Pages/Orders.razor` / order detail component (status actions)



# 🎨 Dev Story: Cashier Flow Clickable Prototype

**Status:** Done  
**Date Created:** 2026-03-07  
**Owner:** Dev Agent  
**User:** Chicuong  
**Story Key:** S1-09-cashier-prototype

---

## Story

As a **cashier**,  
I want a **fast and intuitive interface to take orders**,  
even when the **network is slow or offline**.

---

## Scope

### In Scope
- Interactive prototype in Blazor (`/prototype/cashier`)
- Table selection view (Grid of tables with statuses)
- Order entry view (Product list, search, modifiers)
- Order summary (Cart, total, checkout button)
- Offline mode visual feedback (Warning banner/indicator)
- Mock transitions: Table -> Add Items -> Pay -> Success

### Out of Scope
- Backend integration (Mock data only)
- Real printing
- Real payment gateway integration
- Persistent state (Prototypes resets on refresh)

---

## Dependencies

- S1-03 — Role-permission matrix (to inform what's visible)

---

## Acceptance Criteria

- [x] **AC1**: Prototype flows through a full order: Table Select -> Add Item -> Pay.
- [x] **AC2**: Quick search/filtering for products is interactive.
- [x] **AC3**: Offline status is explicitly visible in the UI via a state toggle.
- [x] **AC4**: UI adheres to modern "Rich Aesthetics" (Glassmorphism, smooth transitions).

---

## Task Breakdown

### TASK 1 — Prototype Foundation (AC3, AC4)
- [x] Create `PrototypeLayout.razor` with glassmorphism sidebar and status bar.
- [x] Implement `ConnectionStatus.razor` component with "Online/Offline" toggle.

**Files:**
- `src/GoiMon.Staff/Shared/PrototypeLayout.razor`
- `src/GoiMon.Staff/Features/Prototype/Components/ConnectionStatus.razor`

### TASK 2 — Table Selection (AC1)
- [x] Implement `TableGridView.razor` with mock table data (Occupied, Available, Reserved).

**Files:**
- `src/GoiMon.Staff/Features/Prototype/Views/TableGridView.razor`

### TASK 3 — Order Entry (AC1, AC2)
- [x] Implement `OrderingView.razor` with category tabs and product search.
- [x] Implement `CartPanel.razor` for reviewing items before "payment".

**Files:**
- `src/GoiMon.Staff/Features/Prototype/Views/OrderingView.razor`

### TASK 4 — Payment & Success (AC1)
- [x] Implement simplified `PaymentModal.razor` (Cash/Bank Transfer) within OrderingView.
- [x] Success state animation/view.

**Files:**
- `src/GoiMon.Staff/Features/Prototype/Views/OrderingView.razor`

---

## Verification Plan

- [ ] Build: `dotnet build src/GoiMon.Staff`
- [ ] Manual: Navigate to `/prototype/cashier`
- [ ] Manual: Add 3 items, toggle offline, observe UI behavior, click "Pay".

---

## Dev Notes

### Design Decisions
1. Use **Tailwind CSS** (if available) or **Vanilla CSS** with a curated dark-mode palette.
2. Emphasize **touch-friendly** targets (large buttons, gestures).
3. Use `MockDataService` to avoid GraphQL calls for this prototype.

---

## Dev Agent Record

### Implementation Plan
1. Setup prototype routing and layout.
2. Build connection status toggle.
3. Build the table grid.
4. Build the product selection and cart.
5. Build the payment simulation.

---

## File List

_Updated during execution_

---

## Change Log

- 2026-03-07 — Story created by Dev Agent.

# 🚀 Dev Story: Viral Growth Foundation (Referral + Share + Streak)

**Status:** Ready  
**Date Created:** 2026-03-05  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 5-1-viral-growth-foundation

---

## Story

As a **merchant owner**,  
I want to **enable built-in viral loops (customer referral, shareable receipt, repeat-visit streak rewards)**,  
so that **customers bring in new customers with minimal ad spend**.

---

## Scope

### In Scope
- Customer referral code generation and redemption tracking
- Shareable digital receipt link with social-share CTA
- Repeat-visit streak logic (simple counter + reward threshold)
- Merchant configuration toggles for viral features (on/off + basic reward values)
- Basic analytics counters: invites sent, referrals converted, streak rewards redeemed

### Out of Scope
- Complex loyalty tier systems
- Deep social network APIs (TikTok/Facebook native SDK integration)
- Fraud scoring engine and anti-abuse machine learning
- Cross-store/global campaign orchestration

---

## Dependencies

- Order and payment completion flow (`3-2-order-payment`) to trigger reward events
- Table/order lifecycle consistency (`3-3-table-management-core`) for dine-in flow parity
- Existing authentication and role guard (`Owner/Staff`)
- StrawberryShake GraphQL generation workflow for new operations

---

## Acceptance Criteria

- [ ] **AC1**: System issues unique customer referral codes and tracks successful redemptions per merchant.
- [ ] **AC2**: Completed order generates a shareable receipt page containing a referral CTA and merchant short link.
- [ ] **AC3**: System tracks customer visit streaks and applies configured reward when threshold is reached.
- [ ] **AC4**: Owner can configure feature toggles and reward parameters from a simple settings page.
- [ ] **AC5**: Dashboard shows core viral metrics: invites shared, referrals converted, streak rewards redeemed.
- [ ] **AC6**: API + Client builds pass; manual flow verifies referral -> new order -> conversion attribution.

---

## Task Breakdown

### TASK 1 — Domain/API (AC: #1, #2, #3)
- [ ] Add entities for referral code, referral redemption, and streak state.
- [ ] Add mutations/queries for code generation, redemption, and streak updates.
- [ ] Wire order-paid event to trigger receipt-share + streak evaluation.

**Files:**
- `src/GoiMon.Api/Features/Growth/*`
- `src/GoiMon.Api/Domain/Entities/*` (new growth entities)
- `src/GoiMon.Api/Features/Orders/*` (event trigger integration)

### TASK 2 — Client/UI (AC: #2, #4, #5)
- [ ] Add owner growth settings screen for toggles/reward values.
- [ ] Add shareable receipt UI block with referral CTA.
- [ ] Add viral metrics card section on owner dashboard.

**Files:**
- `src/GoiMon.Staff/Pages/Growth.razor`
- `src/GoiMon.Staff/Features/Growth/Components/*`
- `src/GoiMon.Staff/Pages/Checkout.razor` or receipt-related component

### TASK 3 — GraphQL + State (AC: #1, #3, #4, #5)
- [ ] Add GraphQL operations for growth settings, referral, streak, and metrics.
- [ ] Add `GrowthUiState` store and register it in client startup.

**Files:**
- `src/GoiMon.Staff/GraphQL/Growth/*.graphql`
- `src/GoiMon.Staff/State/GrowthUiState.cs`
- `src/GoiMon.Staff/Program.cs`

### TASK 4 — Validation/Tests (AC: #6)
- [ ] Add API tests for referral redemption idempotency and streak threshold logic.
- [ ] Add manual scenario checklist for end-to-end viral loop validation.

**Files:**
- `tests/GoiMon.Api.Tests/Features/Growth/*`
- `tests/GoiMon.Client.Tests/Features/Growth/*` (if test style exists)

---

## Verification Plan

- [ ] Build command(s):
  - `dotnet build src/GoiMon.Api/GoiMon.Api.csproj`
  - `dotnet build src/GoiMon.Staff/GoiMon.Staff.csproj`
- [ ] Manual test scenario(s):
  - Existing customer shares receipt link, new customer opens link, applies referral code, completes order, conversion is counted.
  - Returning customer reaches streak threshold and receives configured reward.
- [ ] Edge case(s):
  - Prevent double-counting on repeated redemption attempts for same qualifying order.
  - Handle anonymous customer path (no login) with device/session-safe attribution fallback.

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
- [ ] Status and folder updated to matching state
- [ ] Story board row updated

---

## Dev Notes

### Design Decisions
1. Start with 3 high-leverage loops only: referral, shareable receipt, and streak reward.
2. Keep owner controls simple to fit GoiMon’s low-friction operations.
3. Reuse existing order completion events instead of introducing a separate campaign engine.

### Risks
- Referral abuse can inflate metrics without basic idempotency/eligibility rules.
- Attribution may be imperfect for anonymous users if session continuity breaks.

### Open Questions
- Should reward be fixed amount (`VNĐ`) or percentage in MVP?
- Should referral reward be one-sided (new customer only) or two-sided (referrer + referee)?

---

## Change Log

- 2026-03-05 — Story created in `ready/` by Mary (Business Analyst)

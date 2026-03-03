# 🍱 Dev Story: Combo Management (CRUD + Combo Items)

**Status:** Done (implemented 2026-03-04)  
**Date Created:** 2026-03-04  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 1-3-combo-management

---

## Story

As a **menu admin**,  
I want to **create and manage combos with item composition**,  
so that **staff can sell predefined bundles efficiently**.

---

## Scope

### In Scope
- Combo CRUD
- Add/update/remove combo item
- Variant validation on combo item
- Combo list and by-id query

### Out of Scope
- Dynamic combo builder at checkout
- Promotion/coupon stacking

---

## Acceptance Criteria

- [x] **AC1**: API supports combo create/update/delete
- [x] **AC2**: API supports add/update/remove combo item
- [x] **AC3**: Combo item validates variant ownership and active state
- [x] **AC4**: API exposes combo list query with paging/filter/sort
- [x] **AC5**: API exposes combo-by-id query with items
- [x] **AC6**: Client has combo management and query operations

---

## Implementation Evidence

- Combo mutations: `src/GoiMon.Api/Features/Combos/ComboMutations.cs`
- Combo queries: `src/GoiMon.Api/Features/Combos/ComboQueries.cs`
- Validators: `src/GoiMon.Api/Features/Combos/Validators/`
- Client operations: `src/GoiMon.Client/GraphQL/mutations/ComboMutations.graphql`, `src/GoiMon.Client/GraphQL/queries/GetCombos.graphql`
- Client page: `src/GoiMon.Client/Pages/Combos.razor`

---

## Notes

- This story covers combo catalog management. Ordering combos at checkout is tracked separately in `3-1-order-combo`.

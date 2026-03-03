# 🧾 Dev Story: Product Management (CRUD + Variants + Modifiers)

**Status:** Done (implemented 2026-03-04)  
**Date Created:** 2026-03-04  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 1-2-product-management

---

## Story

As a **menu admin**,  
I want to **manage products with variants and modifiers**,  
so that **ordering options reflect real menu configurations**.

---

## Scope

### In Scope
- Product CRUD
- Product image upload via upload service
- Product list and by-category query
- Product variant CRUD
- Modifier group CRUD
- Modifier option CRUD

### Out of Scope
- Inventory/stock control
- Advanced pricing campaigns

---

## Acceptance Criteria

- [x] **AC1**: Product create/update/delete operations are available
- [x] **AC2**: Product list query supports paging/filter/sort
- [x] **AC3**: Product-by-id query is available
- [x] **AC4**: Product variant CRUD operations are available
- [x] **AC5**: Modifier group CRUD operations are available
- [x] **AC6**: Modifier option CRUD operations are available
- [x] **AC7**: Product image upload flow supported in mutation inputs

---

## Implementation Evidence

- Product mutations: `src/GoiMon.Api/Features/Products/ProductMutations.cs`
- Product queries: `src/GoiMon.Api/Features/Products/ProductQueries.cs`
- Variant/modifier mutations: `src/GoiMon.Api/Features/Products/ProductVariantMutations.cs`
- Validators: `src/GoiMon.Api/Features/Products/Validators/`
- Client operations: `src/GoiMon.Client/GraphQL/mutations/ProductMutations.graphql`, `src/GoiMon.Client/GraphQL/queries/GetProducts.graphql`, `src/GoiMon.Client/GraphQL/queries/GetProductConfigurator.graphql`
- Client page: `src/GoiMon.Client/Pages/Products.razor`

---

## Notes

- Existing implementation already covers core product configurator capabilities used by checkout/order flows.

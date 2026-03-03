# 🗂️ Dev Story: Category Management (CRUD + Query)

**Status:** Done (implemented 2026-03-04)  
**Date Created:** 2026-03-04  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 1-1-category-management

---

## Story

As a **store manager**,  
I want to **manage product categories**,  
so that **menu items are organized and easy to maintain**.

---

## Scope

### In Scope
- Create category
- Update category name
- Delete category
- Query category list (cursor + offset paging)
- Query single category by id

### Out of Scope
- Category hierarchy/tree
- Category image/icon assets

---

## Acceptance Criteria

- [x] **AC1**: API supports `createCategory(input)` mutation
- [x] **AC2**: API supports `updateCategory(input)` mutation
- [x] **AC3**: API supports `deleteCategory(id)` mutation
- [x] **AC4**: API supports paged list query for categories
- [x] **AC5**: API supports `categoryById(id)` query
- [x] **AC6**: Filtering/sorting supported on list query

---

## Implementation Evidence

- API mutations: `src/GoiMon.Api/Features/Categories/CategoryMutations.cs`
- API queries: `src/GoiMon.Api/Features/Categories/CategoryQueries.cs`
- Validators: `src/GoiMon.Api/Features/Categories/Validators/`
- Client operations: `src/GoiMon.Client/GraphQL/mutations/CategoryMutations.graphql`, `src/GoiMon.Client/GraphQL/queries/GetCategories.graphql`, `src/GoiMon.Client/GraphQL/queries/GetAllCategories.graphql`
- Client page: `src/GoiMon.Client/Pages/Categories.razor`

---

## Notes

- This story is treated as completed based on existing API and client integration presence.

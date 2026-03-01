# GraphQL CRUD Test Documents

Endpoint: `http://localhost:5100/graphql`

Files in this folder:

- `fragments.graphql` — reusable fragments for Product, Category, Order, Combo.
- `category-crud.graphql` — Create / Read / Update / Delete flows for Category.
- `category-vars.json` — example variables for category operations.
- `product-crud.graphql` — Create / Read flows for Product (single + bulk) and queries.
- `product-vars.json` — example variables for product operations.
- `order-crud.graphql` — 10 test operations for Orders: 4 happy-path creates + 6 validation error cases (snapshot model).
- `order-vars.json` — variables for all 10 order test cases (TC-01 to TC-10).
- `combo-crud.graphql` — Full CRUD for ProductCombo: create (with/without items), update, replace items, add/remove single item, delete.
- `combo-vars.json` — example variables for combo operations.

Usage examples (curl):

Create a category:
```bash
curl -sS -X POST http://localhost:5100/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"mutation CreateCategory($input: CreateCategoryInput!) { createCategory(input: $input) { id name } }","variables":{"input":{"name":"Beverages"}}}'
```

Run documents with variables file (example for `category-crud.graphql`):
```bash
# Send a specific operation from a multi-operation document using the operationName field
curl -sS -X POST http://localhost:5100/graphql \
  -H "Content-Type: application/json" \
  -d @category-vars.json
```

Notes:
- Replace placeholder IDs in the example variable files with actual IDs returned by create operations when chaining requests.
- The server must be running locally at port 5100 for the examples above. Adjust the URL if different.

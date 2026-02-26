# GoiMon Tenancy Demo

Lightweight demo showing tenant-aware GraphQL using a header `x-tenant-id` and HotChocolate.

Prerequisites
- .NET 8 SDK

Run

```bash
cd _bmad-output/implementation-artifacts/tenancy/demo
dotnet restore
dotnet run
```

Testing the demo
- Open GraphQL Playground at http://localhost:5000/playground
- Add header `x-tenant-id: tenant-demo` in the Playground HTTP Headers.

Example queries

Query items:

```graphql
query {
  items {
    itemId
    name
    priceCents
  }
}
```

Create an order:

```graphql
mutation {
  createOrder(input: { items: [{ itemId: "i1", qty: 1, unitPriceCents: 30000 }] }) {
    orderId
    totalCents
    status
  }
}
```

Notes
- This demo is intentionally minimal and uses in-memory data stores. Use the earlier DDL and sync-envelope specs for a production implementation.

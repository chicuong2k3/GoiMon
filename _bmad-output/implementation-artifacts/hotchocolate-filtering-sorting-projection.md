HotChocolate — Filtering, Sorting, and Projections (v15)

Reference docs:

- Filtering: https://chillicream.com/docs/hotchocolate/v15/fetching-data/filtering/
- Sorting: https://chillicream.com/docs/hotchocolate/v15/fetching-data/sorting/
- Projections: https://chillicream.com/docs/hotchocolate/v15/fetching-data/projections/

Overview

- Filtering: Server-side filtering using `[UseFiltering]` to allow clients to specify filter expressions. Works well with EF Core when combined with `IQueryable`.
- Sorting: Server-side sorting using `[UseSorting]` to accept order expressions from clients.
- Projections: Server-side projection using `[UseProjection]` to translate GraphQL selections into efficient EF Core projections, avoiding overfetching.

Examples (GraphQL)

1) List products in a category, sorted by price (ascending), projecting only `id`, `name`, and `priceCents`:

query {
  products(where: { category: { eq: "Beverages" } }, order: { priceCents: ASC }) {
    id
    name
    priceCents
  }
}

2) Use pagination, projection, filtering and sorting on combos (cursor paging):

query {
  combos(first: 10, where: { name: { contains: "Lunch" } }, order: { priceCents: DESC }) {
    nodes {
      id
      name
      priceCents
      items {
        id
        qty
        product {
          id
          name
          priceCents
        }
      }
    }
    totalCount
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}

3) Filtering with logical operators and ranges (example for products):

query {
  products(where: { priceCents: { gte: 500, lte: 2000 }, category: { in: ["Food", "Snacks"] } }) {
    id
    name
    priceCents
    category
  }
}

Notes and tips

- Ensure resolvers that return `IQueryable<T>` are annotated with `[UseProjection]`, `[UseFiltering]`, and `[UseSorting]` to let HotChocolate translate GraphQL selections into optimized SQL via EF Core.
- Use DataLoaders for nested object resolution (e.g., `product` on `ProductComboItem`) to avoid N+1 queries; batch loaders combine well with projections when projecting IDs first.
- When using paging (`[UsePaging]`), prefer returning `IQueryable<T>` and add `IncludeTotalCount = true` if you need counts.

See the linked docs for advanced configuration, custom filter types, and granular control over which fields support filtering or sorting.

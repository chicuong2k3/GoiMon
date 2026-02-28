# GoiMon.Api — Local dev README

This README shows quick commands to run the API locally (implementation-first GraphQL), manage EF migrations, and example GraphQL queries/mutations you can use with Banana Cake Pop, GraphiQL or `curl`.

Prerequisites
- .NET 8 SDK and runtime installed (project targets `net8.0`)
- PostgreSQL available and the connection string set in `appsettings.json` or environment variable `ConnectionStrings__DefaultConnection`
- (optional) `dotnet-ef` tools: `dotnet tool install --global dotnet-ef`

Run the API locally

Start the API on http://localhost:5000:

```bash
ASPNETCORE_URLS=http://localhost:5000 dotnet run --project src/GoiMon.Api -c Debug
```

Health check

```bash
curl http://localhost:5000/health
# => ok
```

EF Core migrations (when you change domain models)

Create a migration (from repo root). Migrations are stored in `src/GoiMon.Api/Data/Migrations`:

```bash
dotnet ef migrations add Init -p src/GoiMon.Api -s src/GoiMon.Api --output-dir Data/Migrations
```

Apply migrations to the database:

```bash
dotnet ef database update -p src/GoiMon.Api -s src/GoiMon.Api
```

GraphQL endpoint
- URL: `http://localhost:5000/graphql`
- Use Banana Cake Pop (recommended) or any GraphQL client to explore schema and run queries.

Sample queries

- List products (server-side projection/filtering/sorting available):

```graphql
query {
  products {
    id
    name
    priceCents
    category
  }
}
```

- Products by category:

```graphql
query {
  productsByCategory(category: "Noodles") {
    id
    name
    priceCents
  }
}
```

- Products by enum category (demonstrates enum argument):

```graphql
query {
  productsByCategoryEnum(category: Noodles) {
    id
    name
    priceCents
    category
  }
}
```

- Single product by id:

```graphql
query {
  productById(id: "PUT-GUID-HERE") {
    id
    name
    priceCents
    category
  }
}
```

Sample mutations

- Add product:

```graphql
mutation {
  addProduct(input: { name: "New Dish", priceCents: 35000, category: "Specials" }) {
    id
    name
    priceCents
  }
}
```

- Add multiple products (demonstrates list input):

```graphql
mutation {
  addProducts(inputs: [
    { name: "Noodle A", priceCents: 12000, category: "Noodles" },
    { name: "Drink B", priceCents: 5000, category: "Drinks" }
  ]) {
    id
    name
    category
  }
}
```

GraphQL type notes (concepts demonstrated):

- Arguments: resolver method parameters (e.g., `productsByCategory(category: "Noodles")`).
- Input object types: objects passed as arguments (e.g., `input: { name: "X", priceCents: 1000, category: "Y" }`).
- Lists: pass arrays of input objects (e.g., `addProducts(inputs: [...])`).
- Non-null: mark fields/args as required in queries by omitting `null`; HotChocolate infers nullability from CLR types. For explicit non-null wrappers use schema types.
- Enums: server CLR enums (e.g., `ProductCategory`) are exposed as GraphQL enums and can be used as arguments (see `productsByCategoryEnum`).

- Create order:

```graphql
mutation {
  createOrder(input: {
    items: [
      { productId: "PRODUCT-GUID-1", qty: 2, unitPriceCents: 40000 },
      { productId: "PRODUCT-GUID-2", qty: 1, unitPriceCents: 25000 }
    ]
  }) {
    id
    status
    totalCents
  }
}
```

Curl examples

```bash
# Query
curl -X POST http://localhost:5000/graphql \ 
  -H "Content-Type: application/json" \ 
  -d '{"query":"query { products { id name priceCents } }"}'

# Mutation
curl -X POST http://localhost:5000/graphql \ 
  -H "Content-Type: application/json" \ 
  -d '{"query":"mutation { addProduct(input:{name:\"X\",priceCents:1000,category:\"Y\"}){id name}}"}'
```

Notes
- This project uses an implementation-first approach: GraphQL types and fields are inferred from CLR types and resolver signatures.
- If you add custom scalars or explicit `ObjectType<T>` classes, register them in `Program.cs`.
- If you want, I can generate the schema SDL and example queries into `_bmad-output`.

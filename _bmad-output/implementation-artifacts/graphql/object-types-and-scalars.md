HotChocolate v15 — Object Types & Scalars (GoiMon)

1) Object Types (ObjectType<T>)
- Concept: `ObjectType<T>` lets you configure how a CLR type maps to the GraphQL schema. Use it when you want explicit control over field names, descriptions, nullability, types, and nested type wiring.

- How we used it in this repo:
  - `src/GoiMon.Api/GraphQL/Types/ProductType.cs` — configures `Product` fields, descriptions, and types (Id, Name, PriceCents, Category).
  - `src/GoiMon.Api/GraphQL/Types/OrderType.cs` — configures `Order` with `Items` as a non-null list and includes descriptions.
  - `src/GoiMon.Api/GraphQL/Types/OrderItemType.cs` — configures `OrderItem` fields.

- Advantages:
  - Schema clarity: descriptions, non-null constraints, and explicit field types are visible in the generated schema.
  - Fine-grained control: you can rename fields, apply deprecations, or add resolver delegates.
  - Works with `UseProjection/UseFiltering/UseSorting` — HotChocolate still applies middleware over the returned `IQueryable`.

- Example: rename or deprecate a field
```
descriptor.Field(p => p.PriceCents)
          .Name("price")
          .Type<NonNullType<IntType>>()
          .Description("Amount in cents")
          .Deprecated("Use money object instead");
```

2) Scalars
- Concept: Scalars are leaf values in GraphQL (String, Int, Boolean, ID, etc.). HotChocolate lets you add custom scalars to represent domain-specific primitives (e.g., `Money`, `Currency`, `PhoneNumber`).

- Why add a scalar:
  - Normalize wire representation (e.g., always send money as string "400.00" or as integer cents).
  - Centralize parsing/validation and serialization logic.

- Example scalar (conceptual snippet)

```csharp
// Conceptual example - do not drop-in without vetting signatures for v15
public class MoneyType : ScalarType<int, StringValueNode>
{
    public MoneyType() : base("Money") { }

    public override IValueNode ParseResult(object? resultValue) => new StringValueNode(Serialize(resultValue));
    public override object? ParseLiteral(IValueNode literal) =>
        literal is StringValueNode s ? (int)(decimal.Parse(s.Value) * 100) : null;

    public override object? Serialize(object? runtimeValue)
    {
        if (runtimeValue is int cents)
            return (cents / 100m).ToString("0.00");
        return null;
    }
}
```

- In this repo: I did not add a runtime scalar implementation to avoid subtle API mismatches across HotChocolate minor versions. If you want it, I can implement and register `MoneyType` and add tests demonstrating serialization/deserialization.

3) How this ties to our implementation
- We chose DB-backed `IQueryable<T>` resolvers (e.g., `Products()` and `Orders()`), and explicit `ObjectType<T>` classes give us clear schema descriptions for those DB-backed fields.
- Scalars are optional but helpful for domain primitives (money, phone numbers, tenant ids). Add them when you need consistent wire formats and validation.

4) Next steps I can take for you
- Implement a `MoneyType` scalar and register it in `Program.cs` with tests. 
- Generate the schema SDL and example queries for the `ObjectType` classes. 
- Convert some resolvers to `ObjectType`-based resolvers if you need field-level custom resolvers.

Which of these next steps would you like?

---

Implementation-first approach (what we now use)

- What it means: HotChocolate will infer the GraphQL schema directly from your CLR types and resolver signatures (no explicit ObjectType<T> classes required). This is "implementation-first" or code-first schema generation.

- Why we chose it here: faster iteration, fewer files to maintain, and a closer 1:1 mapping between domain model and schema while still supporting projection/filtering/sorting for IQueryable resolvers.

- How to document and control the inferred schema:
  - Use XML doc comments on your domain types/properties. HotChocolate will include those summaries as descriptions in the generated schema.
  - Use attributes when you need explicit control: `[GraphQLName("someName")]` or `[GraphQLDescription("..." )]` from HotChocolate.Annotations.
  - Nullability: rely on C# nullable reference types and value types to influence GraphQL nullability. For finer control use attributes or add ObjectType<T> selectively.

- When to reintroduce ObjectType<T>:
  - Need custom field names, deprecations, or resolver delegates.
  - Need to change GraphQL type shapes that differ from CLR (e.g., rename, compute-only fields).
  - In those cases, implement a single `ObjectType<T>` and register it; you can mix implementation-first and explicit types.

- Inspecting the generated schema and SDL:
  - Run the API and open your GraphQL IDE (Banana Cake Pop or GraphiQL) against `/graphql` to view the generated SDL and try queries.
  - You can also export SDL programmatically with HotChocolate's schema APIs if you need a file artifact.

- Quick tip: Add XML documentation generation in the API project (.csproj) so your comments are embedded and appear in the schema automatically.

This repo uses implementation-first by default; request any explicit ObjectType or scalar if you want targeted schema control.
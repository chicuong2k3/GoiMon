**Aggregate Root: `IAggregateRoot`**

- **Purpose:** Marker/interface for aggregate roots in the domain model. Aggregate roots are the authoritative entity for their consistency boundary; all modifications to child entities should go through the root.

- **Implementation in this repo:**
  - Interface: `src/GoiMon.Api/Domain/IAggregateRoot.cs`
  - Implemented by: `src/GoiMon.Api/Domain/Entities/Order.cs` and `src/GoiMon.Api/Domain/Entities/Product.cs`

- **Design notes:**
  - `IAggregateRoot` exposes `Id { get; }` to allow generic repository constraints while keeping control of setters private on domain types.
  - `Order` encapsulates `OrderItem` creation/removal via `AddItem`/`RemoveItem`, preserving invariants (e.g., `OrderId` set on items and totals recalculated).
  - Keep repository operations (persistence) outside the aggregate; aggregates expose behavior, not persistence concerns.

- **Why this matters for BMAD workflows:**
  - Improves testability (unit tests can verify domain invariants without touching EF).
  - Keeps GraphQL resolvers thin: resolvers call aggregate behavior (via application services or repositories) rather than manipulating object graphs.
  - Aligns with the planned DDD structure in the project (Domain/Application/Infrastructure).

- **Next recommendations:**
  - Add an `IAggregateRoot` marker to any future aggregates (e.g., `Tenant`, `Device`), and consider a shared `AggregateRoot` base if you need common behavior (e.g., domain events).
  - Create unit tests for `Order` behaviors (`AddItem`, `RemoveItem`, totals, invariants).


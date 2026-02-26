**AggregateRoot Base Class**

- **Location:** `src/GoiMon.Api/Domain/AggregateRoot.cs`
- **Purpose:** Provides a common base for aggregate roots in the domain model. It exposes the `Id` property (with protected setter) and a lightweight domain-event collection API (`AddDomainEvent`, `RemoveDomainEvent`, `ClearDomainEvents`, `DomainEvents`).

- **Why add a base class?**
  - Reduces duplication across aggregates (common `Id` handling and event support).
  - Offers a single place to add cross-cutting aggregate behavior later (e.g., domain event dispatch helpers, versioning, concurrency metadata).
  - Works with the existing `IAggregateRoot` marker to constrain repositories and application services.

- **How to use:**
  - Inherit from `AggregateRoot` for aggregate roots (e.g., `Order : AggregateRoot`).
  - Keep child entity constructors/internal state modifications encapsulated and surface behavior through aggregate methods (e.g., `Order.AddItem(...)`).
  - Persist domain events during the repository save operation or via an outbox pattern. Repositories should clear `DomainEvents` after dispatch.

- **Notes for implementation in GoiMon:**
  - `Product` and `Order` now inherit `AggregateRoot` and expose their `Id` via a forwarding property mapping to `base.Id` (keeps EF mapping straightforward while keeping setter protection).
  - EF mapping must map the underlying `Id` property; current `AppDbContext` configurations use the public `Id` on entities and will continue to work.

- **Next recommendations:**
  - Add a domain event base type and a small dispatcher service in `Application` to route events to handlers during repository save.
  - Add unit tests that assert domain events are created when expected and that repository clears them after dispatch.


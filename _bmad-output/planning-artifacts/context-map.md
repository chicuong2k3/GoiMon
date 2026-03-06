# Context Map & Data Flow (Story S1-01)

## 1. Context Relationship Diagram

This describes the strategic relationship between different bounded contexts.

*   `U` = Upstream (Dependency provider / Independent)
*   `D` = Downstream (Dependency consumer)
*   `OHS` = Open Host Service (Standardized API)
*   `ACL` = Anti-Corruption Layer (Translation/Adapter needed)
*   `Conformist` = Faithful adherence to upstream model (copy/paste structure)

### Main Relationships

-   **Catalog Context (U/OHS)** -> `D` **Order Context**
    -   *Rationale*: Order items reference Product IDs and prices from Catalog. Order context doesn't change product definitions.
    -   *Pattern*: Customer-Supplier (Catalog supplies definitions). Order context copies product data (name/price) at the moment of creation (Snapshot pattern) to avoid historical price changes affecting past orders.

-   **Table Context (U)** -> `D` **Order Context**
    -   *Rationale*: Tables exist independently of Orders. An Order requires a valid Table ID (if dine-in).
    -   *Pattern*: Partnership/Shared Kernel (simple ID/Status exchange).

-   **Order Context (U)** -> `D` **Payment Context**
    -   *Rationale*: Payment process starts when an Order is "Ready to Pay". The amount is dictated by the Order.
    -   *Pattern*: Customer-Supplier. Payment needs Order ID and Amount.

-   **Order Context (U)** -> `D` **Inventory Context**
    -   *Rationale*: Confirming an Order triggers stock decrement.
    -   *Pattern*: Domain Event (Async).

-   **Order Context (U) + Payment Context (U)** -> `D` **Invoice Context**
    -   *Rationale*: Invoice generation happens after Payment success and Order finalization.
    -   *Pattern*: Aggregator (combines Order details + Payment ref).

## 2. Cross-Context Events (Domain Events)

These are the public contracts for communication between contexts.

| Event Name | Producer Context | Consumer Context(s) | Trigger Condition | Payload Data |
| :--- | :--- | :--- | :--- | :--- |
| `OrderCreated` | **Order** | Kitchen (Future), Table | Staff punches new order | `OrderId`, `TableId`, `Items[]` |
| `OrderConfirmed` | **Order** | Inventory, Kitchen | Staff sends order to kitchen | `OrderId`, `Items[]` |
| `PaymentInitiated` | **Payment** | External Gateway | User selects method | `OrderId`, `Amount`, `Method` |
| `PaymentSucceeded` | **Payment** | Order, Invoice | Gateway callback checks out | `PaymentId`, `OrderId`, `Amount`, `RefCode` |
| `PaymentFailed` | **Payment** | Order | Gateway rejects / timeout | `PaymentId`, `Reason` |
| `TableOccupied` | **Table** | Order (Validation) | Manager assigns table | `TableId`, `SessionId` |
| `StockLowWarning` | **Inventory** | Notification/Catalog | Item count < threshold | `ProductId`, `CurrentQty` |

## 3. Ownership Matrix

| Context | Primary Owner (Role) | Technical Steward |
| :--- | :--- | :--- |
| **Catalog** | Content Manager / Admin | Backend Lead |
| **Order** | Waitstaff / Cashier | Fullstack Dev (Core) |
| **Table** | Floor Manager | Frontend Lead |
| **Payment** | Cashier / Finance | Senior Backend Dev |
| **Invoice** | Accounting | Backend Dev |
| **Inventory** | Store Manager | Backend Dev |

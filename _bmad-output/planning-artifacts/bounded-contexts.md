# Bounded Contexts Definition (Story S1-01)

## 1. Catalog Context
**Owner**: Content Team / Admin
**Description**: Manages the definition of products, menus, and pricing. This is the source of truth for what can be sold.

### Aggregate Roots & Entities
- **Product**: (AR) Name, Price, Description, Image, CategoryId.
- **Category**: (AR) Name, Icon, SortOrder.
- **Combo**: (AR) Definition of a bundle of products.
- **Modifier**: Options customization for products (e.g., Sugar level, Ice level).

### Key Invariants
- A product cannot be deleted if it is part of an active order (archived instead).
- Prices in Catalog are templates; actual sold price is snapshot in Order Context.
- Modifiers must belong to valid option groups.

### API Surface
- `GetMenu()`: Returns full active menu structure.
- `SyncCatalog()`: Updates local cache.

---

## 2. Order Context
**Owner**: Waitstaff / POS Core
**Description**: Handles the lifecycle of a customer transaction from creation to completion.

### Aggregate Roots & Entities
- **Order**: (AR) OrderId, TableId (optional), Status (Draft, pending, confirmed, completed, cancelled), TotalAmount.
- **OrderItem**: ProductSnapshot (Name, Price), Quantity, Notes, SelectedModifiers.

### Key Invariants
- An order cannot move to 'Completed' without successful Payment (integration w/ Payment Context).
- Order items cannot be modified after sent to kitchen (status: Confirmed) without manager override or specific workflow.
- Total amount must equal sum of line items - discounts + tax.

### Dependencies
- **Upstream**: Catalog Context (for product definitions).
- **Upstream**: Table Context (for table assignment).
- **Downstream**: Kitchen Context (future), Payment Context.

---

## 3. Table Context
**Owner**: Floor Manager
**Description**: Manages physical resources (tables) and their current state.

### Aggregate Roots & Entities
- **Table**: (AR) TableNumber, Capacity, Zone, CurrentStatus (Available, Occupied, CleanUp).
- **Session**: Tracks a group of customers at a table.

### Key Invariants
- A table cannot be assigned to a new order if Status is Occupied.
- Merging tables requires all involved tables to be free or belong to the same session.

### API Surface
- `GetTableMap()`
- `OccupyTable(id)`
- `ReleaseTable(id)`

---

## 4. Payment Context
**Owner**: Cashier / Finance
**Description**: Handles monetary transactions, integration with gateways (QR), and reconciliation.

### Aggregate Roots & Entities
- **PaymentTransaction**: (AR) OrderId, Amount, Currency, Method (Cash, QR, Card), Status (Pending, Success, Failed), ReferenceCode.
- **Refund**: Track returned money.

### Key Invariants
- Payment amount must match specific Order balance for full closure.
- A transaction cannot be modified once Success.

### Dependencies
- **Upstream**: Order Context (provides Amount to charge).

---

## 5. Invoice Context
**Owner**: Accounting
**Description**: Generates legal or customer-facing receipts and tracks tax.

### Aggregate Roots & Entities
- **Invoice**: (AR) InvoiceNumber (Sequential), OrderSnapshot, TaxDetails, Timestamp.

### Key Invariants
- Invoice Numbers must be sequential and gapless (legal requirement).
- Invoice is immutable once issued.

### Dependencies
- **Upstream**: Order Context, Payment Context.

---

## 6. Inventory-lite Context
**Owner**: Store Manager
**Description**: Simple tracking of stock counts for critical items.

### Aggregate Roots & Entities
- **StockItem**: (AR) ProductId, QuantityOnHand, LowStockThreshold.

### Key Invariants
- Reducing stock below zero is allowed but flags a warning (soft inventory).
- Stock is decremented when Order is *Confirmed* (sent to kitchen), not just paid.

---

## 7. Supporting Contexts
- **Identity & Access (IAM)**: Authentication (PIN, Password), Roles (Manager, Staff).
- **Sync/Replication**: Generic mechanism for data synchronization.

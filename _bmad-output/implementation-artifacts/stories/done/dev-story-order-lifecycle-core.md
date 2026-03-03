# 🧾 Dev Story: Order Lifecycle Core (Create + Complete + Cancel + Subscription)

**Status:** Done (implemented 2026-03-04)  
**Date Created:** 2026-03-04  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 3-0-order-lifecycle-core

---

## Story

As a **cashier/staff**,  
I want to **create and manage order status transitions**,  
so that **the store can run an end-to-end serving flow with realtime updates**.

---

## Scope

### In Scope
- Create order with lines (product, variant, modifiers)
- Validation error payload model for order creation
- Complete order transition
- Cancel order transition
- Orders query and order-by-id query
- Realtime subscription for order changes

### Out of Scope
- Mark order paid (tracked separately in `3-2-order-payment`)
- Refund workflow

---

## Acceptance Criteria

- [x] **AC1**: API supports `createOrder` mutation with validation errors
- [x] **AC2**: API supports `completeOrder(orderId)` mutation
- [x] **AC3**: API supports `cancelOrder(orderId)` mutation
- [x] **AC4**: API supports orders list + orderById queries
- [x] **AC5**: API publishes order-changed subscription topic
- [x] **AC6**: Client contains order queries/mutations/subscriptions and orders page

---

## Implementation Evidence

- Mutations: `src/GoiMon.Api/Features/Orders/OrderMutations.cs`
- Queries: `src/GoiMon.Api/Features/Orders/OrderQueries.cs`
- Subscriptions: `src/GoiMon.Api/Features/Orders/OrderSubscriptions.cs`, `src/GoiMon.Api/Features/Orders/OrderSubscriptionTopics.cs`
- Validation: `src/GoiMon.Api/Features/Orders/Validators/CreateOrderInputValidator.cs`
- Client operations: `src/GoiMon.Client/GraphQL/mutations/OrderMutations.graphql`, `src/GoiMon.Client/GraphQL/queries/GetOrders.graphql`, `src/GoiMon.Client/GraphQL/subscriptions/OrderSubscriptions.graphql`
- Client pages: `src/GoiMon.Client/Pages/Checkout.razor`, `src/GoiMon.Client/Pages/Orders.razor`

---

## Notes

- Existing implementation includes combo line support in create order (linked with `3-1-order-combo`).

# ADR-002: Payment Abstraction & Provider Adapter Contract

**Status**: Accepted
**Date**: 2026-03-07
**Context**: GoiMon.Staff (SaaS) needs to support multiple payment methods (Cash, VNPay, VietQR, ZaloPay, Momo) without modifying core business logic.

## Decisions

### 1. Payment Abstraction Layer
-   **Architecture**: `IPaymentProcessor` Interface in Server Domain.
-   **Contract**:
    -   `InitiatePayment(OrderId, Amount, Method)` -> returns `PaymentResponse` (QR Code / Deep Link / Redirect URL).
    -   `VerifyPayment(PaymentTransactionId)` -> returns `PaymentStatus` (Pending / Success / Failed).
    -   `ProcessCallback(Payload)` -> handles webhook from providers.

### 2. Multi-Tenant Payment Strategy
-   **Problem**: SaaS environment. Each Tenant (`TenantId`) has their own Merchant Account (e.g., Momo account).
-   **Solution**: **Tenant-Scoped Configuration**.
    -   Store `PaymentProviderConfig` table (TenantId, ProviderType, ApiKey, SecretKey).
    -   Lookup config by `TenantId` before calling `IPaymentProcessor`.
    -   For MVP: Support **VietQR (Gen QR)** as primary method (low integration cost, high value).

### 3. Payment Flow (Blazor -> Server)
-   **Step 1 (Create)**: Staff clicks "Pay via QR" (Local SQLite -> Sync Queue -> Server).
-   **Step 2 (Response)**: Server calls Provider API -> Returns QR Image URL / String.
-   **Step 3 (Display)**: Blazor displays the QR code.
-   **Step 4 (Wait)**: Blazor subscribes to GraphQL Subscription (`orderPaymentStatusChanged(orderId)`) OR polls every 3s.
-   **Step 5 (Confirm)**: Provider calls webhook -> Server updates Order Status -> Blazor receives update -> Shows "Paid Successfully".

### 4. Direct API vs Redirect
-   **Decision**: **Direct API / QR Display** (Embedded in App).
-   **Except**: E-Wallet Deep Link (Momo App-to-App) on mobile devices.
-   **Constraint**: Avoid full-page redirects if possible to maintain POS context.

## Default Provider Implementation (MVP)
1.  **VietQR (Bank Transfer)**:
    -   Generate standard VietQR string (Napas Standard).
    -   Use `QRCoder` library (Server-side) or JS library (Client-side) to render.
    -   Verification: Manual confirmation by cashier (free) or automated via Bank API (paid/advanced).
    -   **MVP Decision**: Manual Confirmation + Generated QR string.
2.  **Cash**: Immediate local confirmation.

## Consequences
-   **Positive**: Decouples payment logic from order flow. Easy to plug in new methods.
-   **Negative**: Requires careful security (don't expose API keys to client). Webhook handling requires public endpoint.

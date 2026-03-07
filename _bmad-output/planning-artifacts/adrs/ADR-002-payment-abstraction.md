# ADR-002: Payment Abstraction (Provider Adapter Pattern)

## Status
Accepted (Sprint 1)

## Context
GoiMon micro-merchants primarily use Cash, but QR-based payments (VietQR, MoMo, etc.) are increasingly popular. We need a way to support multiple payment methods without tightly coupling our core Order management to any specific provider's SDK.

## Decision
We adopt the **Provider Adapter Pattern** via an `IPaymentProcessor` interface.

### Key Components:
1.  **Payment Initiation**: 
    -   The server generates a `PaymentTransaction` linked to an `OrderId`. 
    -   For MVP, it returns a **dynamic QR URL** (consistent with VietQR standards) or a static MoMo/ZaloPay link.
2.  **Abstraction Interface**: `IPaymentProcessor` with methods `Initiate(Amount, OrderId)`, `Verify(TransactionId)`, and `Refund(TransactionId)`.
3.  **Client-Side Display**: The staff app renders the QR code for customer scanning. No NFC or physical card readers required for MVP.
4.  **Verification Flow**: 
    -   **Primary**: Server-side Webhook for provider callbacks.
    -   **Secondary**: Periodic client-side polling status check via GraphQL for real-time UI updates.

## Consequences
-   **Positive**: Easy to add more QR providers; separates financial logic from ordering logic.
-   **Negative**: Initial development overhead for multiple adapters; dependency on server-to-merchant payment notifications.
-   **Risk**: Delayed callbacks in low-connectivity areas; mitigated by allowing staff to "Manual Confirm" if they witness a successful payment.

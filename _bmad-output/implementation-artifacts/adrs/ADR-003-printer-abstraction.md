# ADR-003: Printer Abstraction & Fallback Behavior

**Status**: Accepted
**Date**: 2026-03-07
**Context**: GoiMon.Staff (Blazor WASM PWA) needs to print receipts (Kitchen Orders, Customer Bills). Current standard: **Direct Thermal Printing (Bluetooth/LAN)** or **Browser Dialog (PDF)**.

## Decisions

### 1. Unified Printer Abstraction
-   **Architecture**: `IPrinterService` (Client-side Abstraction).
-   **Interfaces**:
    -   `PrintReceiptAsync(Order order)` -> returns Task.
    -   `PrintKitchenOrderAsync(Order order)` -> returns Task.
    -   `ConnectAsync()` -> Connect to current printer.
    -   `DisconnectAsync()` -> Close connection.

### 2. Printer Drivers (Client-Side)
-   **Implementation**: **Web Bluetooth API** (Primary for Android/Desktop Chrome).
    -   Uses `navigator.bluetooth` to find Bluetooth printers.
    -   Sends raw **ESC/POS** commands (Standard thermal printer language).
    -   Benefits: Fast, direct, low latency, no extra software needed.
    -   Limitations: Requires HTTPS, User Gesture, Chrome/Edge/Samsung Internet only. iOS Safari **DOES NOT SUPPORT** Web Bluetooth.

### 3. Fallback Mechanism (iOS / Non-Chrome)
-   **Strategy 1 (Primary Fallback)**: **Browser Print Dialog (System Print)**.
    -   Render receipt as invisible HTML/CSS (styled for 58mm/80mm paper).
    -   Call `window.print()` -> User selects AirPrint / System Printer.
    -   *Downside*: Slow, requires multiple clicks.

-   **Strategy 2 (Advanced - Phase 2)**: **Gateway/Proxy App**.
    -   Network Printing: Direct TCP/IP to LAN Printer (Requires CORS proxy or WebSocket bridge).
    -   For MVP: Stick to bluetooth (Android) + System Print (iOS).

### 4. ESC/POS Command Generation
-   **Decision**: **Client-Side Generation**.
-   **Library**: Port `esc-pos-encoder` (JS) or similar C# library to Blazor.
-   **Why**: Can work completely offline. Server doesn't need to know printer details.

## Implementation Plan (MVP)
1.  **Android (Chrome)** -> **Web Bluetooth (ESC/POS)**. Direct connect.
2.  **iOS (Safari)** -> **System Print (HTML/CSS)**. User selects AirPrint printer.
3.  **Laptop (Chrome)** -> **Web Bluetooth** OR **System Print**.

## Consequences
-   **Positive**: Covers 90% of use cases (Android + Bluetooth Printer = Cheap). Works offline.
-   **Negative**: iOS experience is suboptimal (System Print takes ~5 seconds vs Bluetooth < 1s). *Mitigation*: Encourage cheap Android tablets for main POS station.

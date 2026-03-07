# ADR-003: Printer Abstraction (Thermal ESC/POS & AirPrint)

## Status
Accepted (Sprint 1)

## Context
GoiMon merchants need to print receipts and kitchen slips. They use low-cost Bluetooth thermal printers or network-connected receipt printers. These devices vary widely in drivers and capability, especially between Android (Bluetooth friendly) and iOS (AirPrint restrictive).

## Decision
We adopt a **Cross-Platform Printer Abstraction** based on device capability.

### Key Components:
1.  **Primary (Android/Desktop/Chrome)**: **Web Bluetooth API** (ESC/POS) for direct control of thermal printers. This bypasses the OS print dialog and provides fast, paper-cutting, and formatting control.
2.  **Secondary (iOS/Legacy/Desktop Fallback)**: **System Print Dialog** (HTML/CSS) via AirPrint. The browser handles the layout, and the vendor-supplied print dialog manages the device.
3.  **Template Engine**: A central `LayoutEngine` (Razor or Handlebars-based) generates both structured Command-Bytes (ESC/POS) and styled HTML for consistent receipt designs.

## Consequences
-   **Positive**: Works across any device with a modern browser; minimal setup for merchants using standard receipt printers.
-   **Negative**: ESC/POS binary formatting (columns, bold, logo) must be manually implemented for Bluetooth.
-   **Risk**: Apple's Safari has limited Web Bluetooth support, necessitating the AirPrint fallback.
-   **Mitigation**: Standardize on a "Minimal Receipt Format" that works best across both methods.

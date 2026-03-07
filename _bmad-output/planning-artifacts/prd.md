---
stepsCompleted:
  - step-01-init
  - step-02-discovery
  - step-02b-vision
  - step-02c-executive-summary
  - step-03-success
  - step-04-journeys
  - step-05-domain
  - step-06-innovation
    - step-07-project-type
  - step-08-scoping
    - step-09-functional
  - step-10-nonfunctional
  - step-11-polish
  - step-12-complete
inputDocuments:
  - _bmad-output/planning-artifacts/product-brief-GoiMon-2026-02-26.md
workflowType: 'prd'
project_name: GoiMon
author: Chicuong
date: 2026-02-26
documentCounts:
  briefCount: 1
  researchCount: 0
  brainstormingCount: 0
  projectDocsCount: 1
classification:
  projectType: "Blazor WASM SPA (PWA/offline-first, local-gateway option)"
  domain: "Food service — micro-merchants / street vendors"
  complexity: "medium-high"
  recommended_focus_areas:
    - "Offline-first reliability & conflict-resilient sync"
    - "Cross-device compatibility and legacy iOS fallback"
    - "Simple, resilient onboarding for non-technical merchants"
    - "Payment & reconciliation safety (cash-first workflows, QR hooks)"
    - "Local-network pairing & optional gateway hardware"
  classification_source: "User acceptance of advanced elicitation suggestions (2026-02-26)"
---

# Product Requirements Document - GoiMon

**Author:** Chicuong
**Date:** 2026-02-26

## Initialization

The PRD workspace was initialized from the template and the following input documents were loaded:

- _bmad-output/planning-artifacts/product-brief-GoiMon-2026-02-26.md

---

<!-- PRD content to be developed in subsequent steps -->
## Executive Summary

GoiMon is a lightweight, offline-first ordering assistant that replaces paper menus and manual ordering for micro-merchants (street carts, small eateries, beach stalls). It delivers rapid order entry for staff, simple customer self-ordering via QR, and reliable local persistence with later sync—reducing order errors, speeding service, and lowering onboarding friction for low‑tech merchants.

GoiMon’s MVP focuses on a large-button staff UI, simple menu management (CSV import), offline local store with queued sync, and a minimal customer-facing QR menu fallback for legacy devices. The product emphasizes extreme simplicity, low cost of ownership, and robust offline operation so merchants can run reliably in low-connectivity environments and on older devices.

Operationally the product targets micro‑merchant scenarios where device variability, intermittent connectivity, and cash-first reconciliation are the norm. Success will be measured by onboarding time ≤10 minutes, order-entry speed ≤30s during peak, <1% billing errors, and ≥99% offline order persistence. Early pilots will validate device distribution and sync robustness.

### What Makes This Special
- Purpose-built simplicity for non-technical merchants: curated flows, large controls, and CSV-based setup that enable immediate use.
- Offline-first reliability plus lightweight sync and optional local-gateway (Raspberry Pi) for sites without Internet.
- Legacy-device compatibility: Blazor WASM primary UX with a static HTML fallback for older iOS/low-end Android devices to maximize reach.
- Cash-first reconciliation focus: workflows and exports designed to prevent payment disputes and simplify daily reconciliation.

### Project Classification
- Project Type: Blazor WASM SPA (PWA / offline-first, optional local-gateway)
- Domain: Food service — micro-merchants / street vendors
- Complexity: Medium‑high (offline sync, device compatibility, local pairing, reconciliation requirements)

---

<!-- Next: Success Criteria (Step 3) -->
## Success Criteria

### User Success

- Onboarding: New merchant completes initial setup (menu import or manual entry) and records first sale within 10 minutes (target: ≥85% of pilot merchants).
- Order speed: Staff can enter an average order in ≤30 seconds during peak service (measured across sample merchants).
- Billing accuracy: Billing and total calculation errors <1% per merchant-day.
- Reliability: No lost orders during offline periods; local persistence ensures ≥99% order retention until successful sync.

### Business Success

- Early adoption: 200 active merchants within 3 months of pilot launch (pilot -> early-adopter conversion target).
- Retention: 30-day retention of active merchants ≥60%.
- Monetization: Clear path to revenue via low-cost subscription or optional transaction fees; pilot validates willingness-to-pay at target price tiers.
- Operations: Average merchant support requests linked to onboarding ≤10% in first 30 days.

### Technical Success

- Sync reliability: Queued operations successfully sync with backend with ≥98% success rate after connectivity returns (measured by automatic sync logs).
- Compatibility: App functions acceptably (responsive, usable) on target legacy devices identified in pilot (target baseline: iOS 15 / low-end Android models).
- Availability: Core local staff UI usable offline 100% of the time; cloud sync availability target 99% for connected operations.
- Data integrity: No duplicated or inconsistent orders post-sync during pilot (idempotency and conflict-resolution validated).

### Measurable Outcomes

- Onboarding ≤10 minutes: ≥85% merchants in pilot
- Order-entry ≤30s: measured median across peak-hour transactions
- Billing errors <1% per merchant-day
- Offline order persistence ≥99% during offline windows
- Sync success ≥98% within a 24-hour window after connectivity restoration

## Product Scope

### MVP - Minimum Viable Product

- Staff Order UI (large-button, quick modifiers)
- Menu management with CSV import and simple CRUD (Online-only for Admin)
- Offline-first local store with queued operations (Phased: POS in Sprint 8+, Admin stay online)
- Customer-facing QR menu and static HTML fallback for legacy devices
- Basic reconciliation/export for daily cash reconciliation (Online-first)

### Growth Features (Post-MVP)

- Payment processor integrations (optional, post-MVP)
- Inventory/stock tracking and alerts
- Advanced reporting and analytics dashboard
- Multi-store management and advanced permissions

### Vision (Future)

- Seamless multi-device sync with optional low-cost gateway, richer payment options, localization and marketing tools for merchant growth.

---

<!-- Next: User Journey Mapping (Step 4) -->

## User Journeys

### 1. Mr. Nam — Primary User (Staff success path)

- Opening: Morning rush at a street cart; Mr. Nam uses an old Android phone to take orders.
- Steps: Open staff UI → select category → tap items (large buttons) → apply quick modifier → confirm order → print/notify kitchen → mark payment (cash) → complete.
- Climax: During peak, orders queue and totals auto-calc; Anh Nam confirms and clears a 5-item order in ≤30s.
- Resolution: Faster throughput, zero calculation errors, day-end reconciliation matches cash.
- Requirements revealed: large-button staff UI, instant totals, offline persistence, quick modifiers, receipt/print or kitchen notification, daily export for reconciliation.

### 2. Primary Edge Case — Offline→Sync conflict recovery

- Opening: Mr. Tuan (beach stall) takes offline orders during an extended outage on two devices.
- Steps: Device A and Device B both record orders locally → connectivity returns → sync attempts → potential duplicate/order id conflicts.
- Climax: Sync identifies near-duplicate orders; system dedupes via idempotency and merchant confirms ambiguous entries.
- Resolution: No lost orders; merchant resolves one duplicate via simple UI; totals reconcile.
- Requirements revealed: idempotent order IDs, conflict-resolution UI, manual merge/confirm option, robust retry/backoff, audit logs.

### 3. Ms. Hoa — Secondary User (Multi-device & customer QR flow)

- Opening: Small eatery owner with staff; owner pairs a tablet and phone for multi-device views; enables QR per table.
- Steps: Owner sets menu via CSV → staff tablet receives orders → customer scans QR → places self-order → order appears to staff → staff accepts or requests clarification → customer pays online if available or pays at counter.
- Climax: Customer self-order during busy hour reduces staff load while owner maintains control over price/menus.
- Resolution: Smooth pairing, simple pairing flow, optional short-link QR, orders routed correctly.
- Requirements revealed: device pairing (LAN/cloud), CSV import, QR generator, customer-facing lightweight menu, order acceptance flow, permission model.

### 4. Admin/Owner — Reconciliation & Configuration

- Opening: Owner reviews daily totals after close.
- Steps: Open admin dashboard (mobile or web) → view daily sales summary → export CSV → reconcile cash vs. recorded orders → adjust any manual corrections → review sync logs.
- Climax: Owner spots mismatch and uses order audit to find and mark missing payment.
- Resolution: Reconciliation completed quickly; owner confident in records.
- Requirements revealed: daily summary, CSV export, order audit trail, manual correction workflow, access controls.

### 5. Support/Troubleshooter — Incident resolution

- Opening: Merchant reports "missing order" via support channel.
- Steps: Support opens merchant's audit logs → sees device offline window and queued operations → identifies sync error → instructs merchant to trigger manual sync or applies server-side fix → confirms merchant's orders restored.
- Climax: Support resolves without data loss.
- Resolution: Merchant resumes normal operation.
- Requirements revealed: detailed logs, support diagnostics UI or export, manual sync trigger, safe server-side remediation tools.

### Journey Requirements Summary

- Core: large-button staff UI, offline persistence, queued reliable sync, idempotent order model, CSV import, QR menu + static fallback, device pairing, daily exports.
- Ops/Support: audit logs, manual sync tools, support diagnostics, simple fallback instructions for legacy devices.
- UX: ultra-fast ordering, minimal onboarding steps, clear conflict-resolution prompts.

---

<!-- Next: Domain Requirements (Step 5) -->
## Domain-Specific Requirements

### Compliance & Regulatory
- Payments: MVP will avoid direct processor integration; if later added, expect PCI-DSS scope. For MVP with QR-only hooks, document payment boundary and operator responsibilities.
- Data privacy: Treat merchant/customer data per local privacy norms; define retention policy (default: 90 days sales/audit logs, configurable).
- Taxes & receipts: Support configurable tax rates per merchant locale and exportable receipts for reconciliation (local tax rules vary by country).

### Technical Constraints
- Offline-first guarantees: Local store must persist orders and operations reliably; target ≥99% local retention during offline windows.
- Idempotency & ordering: Use stable, device-scoped order IDs and server reconciliation to prevent duplicates on sync.
- Device compatibility: Support low-end Android and iOS 15+ behavior; include static HTML fallback for older iOS.
- Storage & encryption: Store sensitive data (payment tokens if any, merchant credentials) encrypted at rest using platform APIs; require TLS for all server communication.
- Connectivity model: Design for intermittent, high-latency networks; implement exponential backoff and retry queues for sync.

### Integration Requirements
- Optional local-gateway: Define lightweight LAN gateway spec (Raspberry Pi) for truly offline sites — sync bridge, local pairing, and periodic cloud uplink.
- Printing/receipts: Support simple Bluetooth or network printer integrations (common POS printers) or export CSV for external printing.
- Minimal external integrations for MVP: CSV import/export and optional webhook/QR payment hooks; defer full payment-processor SDKs to post-MVP.

### Operational & Environmental Constraints
- Low-cost hosting: Target minimal bandwidth and compute for merchant sync; allow self-hosting or low-cost cloud.
- Harsh environments: Account for salt/wind/exposure — recommend rugged or protected device guidance in docs.
- Supportability: Provide simple manual-sync and diagnostic steps for field support; logs accessible to support staff.

### Risk Mitigations
- Risk: Data loss during sync. Mitigation: durable local write-ahead log, idempotent sync operations, server-side reconciliation tools.
- Risk: Blazor WASM incompatibility on legacy iOS. Mitigation: static HTML fallback, technical spike to test critical devices, and performance budget for WASM features.
- Risk: Merchant confusion leading to reconciliation errors. Mitigation: clear cash-first workflows, daily-export reconciliation UI, and onboarding checklist.

### Measurable Domain Constraints
- Retention: 90-day default retention for sales/audit logs (configurable).
- Sync SLA for pilot: ≥98% successful sync within 24 hours after connectivity restoration.
- Target device baseline: iOS 15 and Android API levels covering common low-end devices (to be validated in pilot).

---

<!-- Next: Innovation Focus (Step 6) -->

## Project-Type (Web App) Requirements

### Key Questions & Responses

- SPA or MPA?: SPA — primary delivery is Blazor WASM PWA for staff; provide static HTML fallback for legacy customer devices.
- Browser support?: Target Chromium-based Android (low-end), iOS 15+ Safari, and desktop Chromium/Firefox for admin. Pilot to validate exact device baseline.
- SEO needed?: ASSUME: not required for core ordering flows (QR-driven); clarify if merchant storefront discovery is needed later.
- Real-time?: Near-real-time locally (kitchen/print) required; cloud sync can be eventual. Consider LAN push or gateway-assisted WebSocket for local notifications.
- Accessibility?: Commit to WCAG AA for merchant UI (large controls, high contrast); customer QR menu should meet basic legibility and touch targets.

### Required Sections (brief)

- Browser matrix: Document supported engines/versions and graceful degradation plan; include known WASM constraints and fallback loading strategy.
- Responsive design: Define breakpoints optimizing for handheld staff devices (360–480px), tablet pairing layouts, and a condensed admin desktop view.
- Performance targets: Set TTI ≤2s from cache for staff UI; order actions local latency <100ms; WASM bundle budgets and lazy-load strategy.
- SEO strategy: No heavy SEO needed for QR menus; reserve prerendered marketing pages or merchant storefronts for later phases.
- Accessibility level: WCAG AA baseline for merchant screens; include accessibility tests in pilot acceptance criteria.

### Technical Architecture Considerations

- Blazor WASM PWA with Service Worker for offline shell and asset caching; static HTML/JS fallback for legacy customers.
- Idempotent REST/sync endpoints with append-only operation logs and reconciliation APIs to handle queued operations from devices.
- Client-side persistence using IndexedDB + write-ahead log (WAL) for durable local writes and crash recovery.
- Sync protocol: stable device-scoped order IDs, vector/timestamp metadata, server-side dedupe and merchant-confirmed conflict UI.
- Optional local-gateway (Raspberry Pi) for LAN pairing, local message brokering, and reliable uplink from offline sites.
- Printer support via pluggable adapters (Bluetooth, network/ESC-POS) and gateway-assisted printing when browser APIs are insufficient.
- Security: TLS for cloud comms, platform-backed encryption at rest for sensitive tokens, least-privilege credential design.

### Top Risks & Mitigations

- WASM performance on legacy iOS: Mitigation — static fallback, device spike tests, strict bundle-size budgets.
- Duplicate/lost orders with multi-device offline use: Mitigation — WAL + idempotent sync + server-side dedupe + conflict UI.
- Merchant reconciliation errors: Mitigation — clear cash-first workflows, daily exports, onboarding checklist and in-app guides.
- Printer/local-notification failures: Mitigation — multiple delivery paths (Bluetooth, gateway, CSV) and diagnostics.
- Data corruption on client: Mitigation — WAL, integrity checks, local backups, recovery routines.
- Gateway security misconfiguration: Mitigation — secure defaults, signed comms, network isolation guidance.

### Prioritized Next Actions

1. Device-compatibility spike: baseline Blazor WASM perf tests on representative low-end Android and iOS 15 devices.
2. Offline-sync prototype: implement WAL + simple server reconcilers to validate idempotency and multi-device conflict scenarios.
3. Gateway pilot: deploy a Raspberry Pi gateway in 3 pilot merchants for LAN pairing and local notifications testing.
4. Printer integration tests: validate Bluetooth and network printing across common POS printers; build adapters.
5. Onboarding usability tests: 5 moderated merchant sessions to validate ≤10-minute onboarding and reconciliation flows.
6. Security & retention: define retention, encryption, backup policy and run resiliency tests for ≥99% local persistence.

---

<!-- Next: Scoping (Step 8) -->

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Problem-solution and experience MVP focused on delivering immediate operational value to micro-merchants: reliable offline order entry, accurate billing, and a frictionless onboarding that proves the core hypothesis (merchants will adopt a low-cost, offline-capable ordering helper).

**Resource Requirements (MVP team):** Small cross-functional team: 1 product lead/PM, 1 full-stack engineer with Blazor/GraphQL experience, 1 frontend engineer (WASM/JS fallbacks), 1 QA/devops engineer for offline/sync testing; optionally 1 UX researcher for pilot onboarding sessions.

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:** Staff order entry (Anh Nam), basic customer self-order via QR (Chị Hoa), offline resilience and sync recovery (Mr. Tuan), and owner reconciliation and export (Admin/Owner).

**Must-Have Capabilities:**
- Large-button staff order UI with quick modifiers and instant totals.
- Menu management: CSV import and simple CRUD for items and categories.
- Offline-first client store with write-ahead log and queued sync.
- Stable, idempotent order model and basic conflict-resolution UI.
- Customer-facing QR menu (lightweight static HTML fallback for legacy devices).
- Daily export (CSV) and basic reconciliation UI for owners.
- Basic printer support or CSV print fallback.

### Post-MVP Features (Phase 2)

- Optional QR/processor payment integration hooks and reconciliation automation.
- Multi-device pairing and richer permission model (owner/staff roles).
- Better device management and OTA updates for local gateway.
- Basic analytics and item-level reporting for merchant insights.

### Expansion Features (Phase 3)

- Full payment-processor integrations and in-app payments.
- Inventory and stock tracking with simple alerts.
- Localization, reseller onboarding tools, low-cost gateway marketplace (Raspberry Pi appliance).
- Marketing tools (coupons, simple loyalty) and multi-store management.

### Risk Mitigation Strategy

**Technical Risks:**
- WASM/device compatibility — run device-compatibility spike early; provide static fallback.
- Offline sync complexity — validate WAL + server reconciliation in a prototype.

**Market Risks:**
- Merchant adoption — pilot with 10–20 merchants, measure onboarding time and reconciliation pain points; iterate UX.

**Resource Risks:**
- Limited engineering capacity — sequence spikes first (compatibility, sync, gateway) to de-risk before building full feature set; consider contracting for gateway pilot.

---

<!-- Next: Functional Requirements (Step 9) -->

## Functional Requirements

### Order Management (Core)

- FR1: [Staff] can create a new order and add one or more menu items.
- FR2: [Staff] can modify an item in an order with simple modifiers (size, add-ons).
- FR3: [Staff] can remove items from an order prior to finalization.
- FR4: [Staff] can view the running total (taxes/fees applied) for the order before confirming.
- FR5: [Staff] can confirm and queue an order for kitchen/fulfillment.

### Payment & Reconciliation

- FR6: [Staff] can mark an order as paid by cash and record the collected amount.
- FR7: [Staff] can mark an order as paid via an external/QR payment and record payment reference.
- FR8: [Owner] can view daily sales summary and export sales as CSV for reconciliation.
- FR9: [Owner] can adjust/correct an order’s payment status with an audit note.

### Menu Management

- FR10: [Owner] can create, update, and delete menu items, categories, and prices.
- FR11: [Owner] can import menu items via CSV and map required fields.
- FR12: [Owner] can publish/unpublish the customer-facing QR menu.

### Customer Self-Order (QR Flow)

- FR13: [Customer] can open a merchant’s QR menu and select items to submit a self-order.
- FR14: [Staff] can receive, review, and accept or request clarification on customer-submitted orders.

### Offline & Sync

- FR15: [System] persists all new and modified orders locally when network is unavailable.
- FR16: [System] queues locally persisted operations and attempts reliable sync when connectivity is restored.
- FR17: [System] exposes a merchant-facing conflict-resolution flow when sync discovers ambiguous/duplicate records.
- FR18: [System] guarantees no acknowledged order is lost after local confirmation (durable write semantics).

### Device Pairing, Notifications & Printing

- FR19: [Owner] can pair an additional device (staff tablet/phone) to receive order updates.
- FR20: [System] can deliver local fulfillment notifications to paired devices (e.g., kitchen).
- FR21: [Staff/Owner] can print or generate a printable receipt/export for an order.

### Admin, Support & Diagnostics

- FR22: [Support] can access an audit log of orders, sync attempts, and conflicts for a merchant.
- FR23: [Support] can trigger or advise a manual sync and see last-sync status/time.
- FR24: [Owner] can configure retention policy and view storage/queue health indicators.

### Security & Access

- FR25: [Owner] can create and manage basic staff accounts and assign simple roles (owner/staff).
- FR26: [System] requires authenticated sessions for owner/staff actions and provides session management appropriate for intermittent connectivity.
- FR27: [System] records an audit trail for order creation, modification, payments, and reconciliation actions.

### Integrations & Extensibility

- FR28: [System] can export and import CSVs for menu and sales data to interoperate with external tools.
- FR29: [System] exposes webhook/configurable hooks for optional payment or backend integrations (no processor implementation required in MVP).

---

<!-- Next: Non-Functional Requirements (Step 10) -->

## Non-Functional Requirements

### Performance

- NFR-P1: Initial interactive time (TTI) for the staff UI should be ≤2s from cache on target low-end devices during normal operation (measured in pilot lab). Service Worker should provide cached shell so staff can open the app instantly.
- NFR-P2: Local order-entry actions (add/remove/modify item) must complete locally within 100ms to preserve perceived responsiveness.
- NFR-P3: WASM bundle incremental download budget should be limited (target: <1.5MB additional payload) with lazy-loading of non-essential modules.

### Reliability & Availability

- NFR-R1: Local persistence durability: client must retain ≥99% of orders created during offline windows (simulated outage tests) until successful sync.
- NFR-R2: Sync success SLA for pilot: ≥98% of queued operations must complete successfully within 24 hours after connectivity restoration.
- NFR-R3: Cloud endpoints should target 99% availability for non-local-critical operations (backups, analytics, long-term sync).

### Security & Data Protection

- NFR-S1: All cloud communications MUST use TLS 1.2+; local gateway communications must be encrypted and authenticated.
- NFR-S2: Sensitive data (payment tokens, credentials) stored on-device MUST use platform-backed encryption where available.
- NFR-S3: Audit trail retention default = 90 days (configurable), with secure deletion routines; owners can export and archive data via CSV.
- NFR-S4: Follow least-privilege access model for staff/owner roles; require authentication for all owner/staff actions and record session events in audit logs.

### Scalability & Capacity

- NFR-SC1: The backend sync design must tolerate pilot-scale concurrency (hundreds of merchants) and be architected to scale horizontally; design must support 10x pilot scale with <10% additional latency.
- NFR-SC2: Gateway design must support at least 5 paired devices in a local site without message loss or ordering issues.

### Accessibility

- NFR-A1: Merchant/staff UI meets WCAG AA baseline for core screens (order entry, reconciliation, onboarding): large touch targets, high-contrast mode, screen-reader labels for critical controls.
- NFR-A2: Customer QR menu must meet basic legibility and touch target guidelines and have a high-contrast option.

### Integration & Maintainability

- NFR-I1: CSV import/export formats must be documented and stable; exports must be compatible with common spreadsheet tools.
- NFR-I2: System must provide diagnostics endpoints/log exports for support to retrieve sync logs and queue status.

---

<!-- Next: Polish Document (Step 11) -->

## Workflow Completion

The PRD for GoiMon has been completed and polished. This document now includes:

- Executive Summary
- Success Criteria
- User Journeys
- Domain-Specific Requirements
- Project-Type Requirements
- Project Scoping & Phased Development
- Functional Requirements (capability contract)
- Non-Functional Requirements (quality attributes)

Next steps you can choose:

- Run implementation readiness validation to check epic coverage and developer readiness. (`/bmad` validation workflows available)
- Proceed to architecture, UX, and epic breakdown using this PRD as the source of truth.
- Pilot execution: start the device-compatibility and offline-sync spikes recommended in the PRD.

If you'd like, I can run the validation workflow now or open the next workflow (architecture). What would you like to do next?

---








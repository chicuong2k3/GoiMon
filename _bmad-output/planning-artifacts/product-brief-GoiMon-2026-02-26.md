---
stepsCompleted: [1, 2, 3, 4, 5]
date: 2026-02-26
author: Chicuong
---

# Product Brief: GoiMon

## Executive Summary

GoiMon helps small food vendors (street carts, small eateries, beach stalls) replace paper menus and manual ordering so staff prepare orders faster, payments are calculated accurately, and customers can self-select items for a more convenient, low-cost experience. The product aims to be competitive with existing solutions while remaining simpler and cheaper for low-tech users.

---

## Core Vision

### Problem Statement

Many small food businesses still use handwritten paper menus and manual order-taking. This leads to slow service, order errors, inconsistent pricing, slow payment handling, and extra training overhead for staff. These businesses need an affordable, simple digital ordering and payment helper that requires minimal technical skill to use.

### Problem Impact

- Longer wait and preparation times during peak hours
- Frequent calculation and billing errors causing revenue loss or customer friction
- Staff time wasted on writing/reading paper orders and handling payments
- Lost upsell and ordering opportunities from customers who could self-serve
- Small vendors avoid digital tools because existing solutions are too complex or expensive

### Why Existing Solutions Fall Short

- Many point-of-sale platforms (e.g., KiotViet) are feature-rich but can be costly and complicated for tiny vendors.
- Complex setup, subscription costs, and training requirements deter street vendors and beach stalls.
- Mobile-first or kiosk solutions often assume reliable connectivity or modern devices, which may not be available.

### Proposed Solution

A lightweight, low-cost ordering assistant that:
- Replaces paper menus with an extremely simple UI for staff and customers.
- Supports quick item selection, grouped combos, and auto-calculation of totals and taxes.
- Provides an optional customer-facing self-order flow (QR code → menu → order → pay).
- Works offline-first (local store) with sync to backend when connectivity is available.
- Requires minimal setup and training — usable on basic smartphones or low-end tablets.
- Backend supports GraphQL for efficient data queries and flexible integrations.

### Key Differentiators

- Purpose-built simplicity for non-technical users (large fonts, minimal screens, guided flows).
- Extremely low total cost of ownership (lightweight hosting, minimal subscription).
- Offline-first UX plus flexible sync for unreliable networks.
- Rapid onboarding: setup menu and pricing in minutes, no paperwork.
- Focus on micro-merchant realities (street location, seaside devices, cash-heavy operations).

### Architecture Assumptions and Constraints

- Frontend: Blazor WebAssembly (WASM) SPA using BlazorBlueprint for accessible UI and EasyAppDev.Blazor.Store for local state/offline sync.
- API: GraphQL endpoint for efficient data fetching and flexible front-end needs.
- Hosting: lightweight cloud or self-hosted backend; offline-first capability via local store and sync.
- Auth & payments: simple PIN or device-bound session for staff; optional QR-based payments or cash reconciliation support.

### Addressing User Concerns

- iOS compatibility (older devices): Blazor WASM may perform poorly or have compatibility issues on very old iOS versions. Provide a graceful fallback:
  - Runtime/SDK strategy: Prefer targeting .NET 8 for maximum compatibility on older devices and provide an option to disable SIMD-heavy features on legacy devices to improve stability and performance.
- No/unstable internet (beach/remote): design offline-first with local persistence and sync:
  - Use EasyAppDev.Blazor.Store for local state and queued operations.
  - Service Worker + cached assets so the app loads from device cache.
  - Support LAN/local-server sync (device acts as a local sync host) or a low-cost gateway (Raspberry Pi) for venues without internet.
  - Allow entirely local operation (take orders and print/reconcile locally) and later sync when connectivity returns.

---

*(Appended: Product vision and architecture notes. Ready for user-persona discovery next.)*

## Target Users

### Primary Users

- **Street Vendor — Mr. Nam**  
  - Role & context: single-owner street food cart, serves breakfast/lunch on busy sidewalks.  
  - Devices: old Android or older iPhone (may be iOS 15 or below).  
  - Needs: minimal setup, immediate speed improvements, cash-first flow, ability to take orders quickly during rush.  
  - Pain points: handwritten notes, calculation errors, lost orders during peak, no reliable internet.  
  - Success: can take orders 2x faster, zero billing errors, reconcile cash at day end with minimal effort.

- **Small Eatery Owner with Staff — Ms. Hoa**  
  - Role & context: small indoor eatery with 2–3 staff, occasional peak hours.  
  - Devices: mixed (one tablet for staff, some old phones).  
  - Needs: easy staff-facing UI, simple menu management, basic reporting and price control, optional customer QR ordering.  
  - Pain points: training staff, inconsistent order entry, reconciling payments.  
  - Success: staff onboarded in minutes, faster table turnaround, easier cash/QR reconciliation.

- **Beach Stall Vendor — Mr. Tuan**  
  - Role & context: seaside stall with intermittent connectivity, exposed to salt/wind, mostly tourist customers.  
  - Devices: cheap Androids or older iPhones; may rotate devices periodically.  
  - Needs: robust offline operation, cached assets, local sync when connection returns, simple QR for tourists if online.  
  - Pain points: no/unstable internet, device limitations, high season traffic spikes.  
  - Success: takes orders offline reliably, syncs sales and prices later, supports occasional online payments.

### Secondary Users

- **Customers** — local regulars and tourists who prefer quick, frictionless ordering (cash when offline, QR/mobile pay when available).  
- **Staff/Helpers** — temporary workers who need a very small learning curve.  
- **Shop Owner / Admin** — manages menu, prices, reconciliation; may be low-technical-skill.

### User Journey (combined flow)

- **Discovery:** Owner hears about GoiMon via word-of-mouth, market seller group, or local reseller.  
- **Onboarding (first-use, 5–10 minutes):** Owner installs/pulls up app; guided quick setup: enter basic menu items/prices or scan a CSV/template; choose offline or cloud mode; generate QR for customers if desired.  
- **Core Usage (daily):**  
  - Staff or owner opens staff UI (large buttons) to register orders; totals auto-calc.  
  - If online and customer prefers, show QR at table/checkout → customer selects items → sends order to staff.  
  - If offline, all orders stored locally; payments taken in cash and marked in-app.  
- **Success Moment:** During a rush, owner sees orders processed without paper, totals calculated correctly, and staff handle throughput smoothly.  
- **Long-term:** Owner uses simple daily summary to reconcile cash vs orders; system syncs to cloud when available; owner may opt for low-cost subscription for backups and multi-device sync.

*(Appended: Target users and journey. Ready to proceed to metrics.)*

## Success Metrics

### User Success Metrics (accepted targets)
- Order speed: Reduce order entry time by 50% during peak (example: from 60s → 30s).
- Billing accuracy: <1% billing errors per day.
- Onboarding time: Average setup ≤ 10 minutes for new merchants.
- Offline reliability: ≥99% local order persistence (no lost orders) during offline periods.

### Business Objectives
- 3 months: 200 active merchants (using daily/weekly).
- 12 months: 2,000 merchants and a path to sustainable revenue via low-cost subscription or optional transaction fees.

### Key Performance Indicators (KPIs)
- Merchant signups/month: 200
- Weekly active merchants (WAU): 60% retention of signups after 30 days
- Orders per merchant/day: 30 (average)
- % orders paid via QR/mobile when online: 30%
- Onboarding success rate (complete setup within 10 minutes): 85%
- Sync success rate (queued operations successfully synced when online): 98%

*(Appended: Success metrics and business KPI targets. Ready to proceed to scope definition.)*

## MVP Scope

### Core Features (MVP)

- **Staff Order UI:** Large-button, single-screen order entry for staff; quick add/remove items, item modifiers, combos, and instant total calculation (taxes/fees).
- **Menu Management:** Simple CRUD for items, categories, prices and combos; import via CSV/template for fast setup.
- **Offline-First Local Store:** Local persistence of menu and orders using EasyAppDev.Blazor.Store; queued operations and conflict-resilient sync when online.
- **Customer Self-Order (QR):** Generate a QR (or short link) that opens a lightweight menu page; customers can select items and submit orders to staff (supports browser fallback for legacy devices).
- **Payment Modes:** Cash-first workflow with manual marking of payments; optional QR/mobile-pay integration hooks (minimal integration in MVP).
- **Order Persistence & Sync:** Reliable local queue of orders with background sync and retry; ability to export daily sales for reconciliation.
- **Onboarding Flow:** Guided first-time setup (enter basic menu, set prices, choose offline/cloud mode) in ≤10 minutes.
- **Basic Reconciliation & Summary:** Daily summary for cash reconciliation, simple sales totals and item counts.
- **Device Pairing / Multi-Device:** Lightweight pairing to allow a staff tablet and owner's phone to see orders (local network or cloud sync, basic permission model).
- **Fallback Static Menu:** Small, non-WASM static HTML+JS menu for legacy devices (iOS 15 and older) to allow ordering without Blazor WASM.

### Out of Scope for MVP

- Inventory management and automated stock tracking
- Advanced reporting and analytics dashboards
- Loyalty programs, coupons, and complex promotions
- Full payment processor integrations (beyond simple QR hooks)
- Multi-store enterprise features and role-based access beyond basic owner/staff
- Rich marketing integrations (SMS campaigns, CRM)

### MVP Success Criteria

- Merchant onboarding: ≥85% complete setup within 10 minutes.
- Core usage: Staff can enter an order in ≤30s on supported devices.
- Reliability: ≥99% order persistence during offline windows; sync success ≥98% when connectivity returns.
- Billing accuracy: <1% billing errors reported in first 90 days.
- Business validation: 200 active merchants within 3 months (pilot/early-adopter target).

### Future Vision (post-MVP)

- Add inventory and ingredient-level tracking for busy kitchens.
- Integrate with major payment processors for seamless in-app payments.
- Advanced reporting, promotions, and simple accounting exports.
- Language/localization for multi-region expansion and tourist support.
- Optional hosted gateway (low-cost hardware) for truly offline-first deployments with local network sync.

*(Appended: MVP scope, out-of-scope, success criteria, and future vision. Presenting menu options next.)*

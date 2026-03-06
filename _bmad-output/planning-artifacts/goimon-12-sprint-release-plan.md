# GoiMon 12-Sprint Release Plan (Compete with iPOS)

## 1) Planning Assumptions

- Sprint length: 2 weeks
- Total duration: 24 weeks (~6 months)
- Team baseline: 1 PM/BA, 1 UX/UI, 3 engineers, 1 QA (shared)
- Primary segment: VN SMB F&B (single store to small chains)
- Target platforms in this phase:
  - Android POS terminal + Android phone/tablet (cashier)
  - Web admin (manager/owner)
- Product strategy: offline-first cashier operations, compliance-ready, migration-friendly

## 2) Product KPIs (North Star + Guardrails)
 
### North Star

- Active paid stores at end of Month 6: >= 20 pilot stores

### Operational KPIs

- Median order-to-payment completion time: <= 60 seconds (simple order)
- Crash-free session rate (cashier app): >= 99.5%
- Offline queue sync success within 5 minutes after reconnect: >= 99%
- Receipt/kitchen print success rate: >= 99%
- P95 report page load (dashboard): <= 2.5 seconds
- First-time onboarding completion (store setup + menu import): <= 2 hours

### Business KPIs

- Month-1 retention from pilot stores: >= 85%
- Weekly active cashier accounts per active store: >= 3
- Support tickets per store/week after stabilization: <= 2

## 3) Release Structure

- Release 1 (Sprints 1-4): POS MVP Foundation + Core cashier flow
- Release 2 (Sprints 5-8): Operations reporting + compliance + reliability hardening
- Release 3 (Sprints 9-12): Inventory lite + multi-branch + pilot launch readiness

## 4) 12-Sprint Detailed Plan

## Sprint 1 — Product/Architecture Foundation

### Backlog Items

- Define bounded contexts: Catalog, Order, Table, Payment, Invoice, Inventory-lite
- Finalize role matrix: Cashier, Supervisor, Manager, Owner, Accountant
- Define offline-first sync contract (local queue, idempotency keys, retries)
- Set up observability baseline (app telemetry, sync metrics, print events)
- UX flows: order entry, bill split, checkout, error/retry states

### Acceptance Criteria

- Architecture decision records approved for sync, payments, printing
- Role-permission matrix mapped to concrete actions in UI and API
- Offline event schema versioned and documented
- Prototype user flow clickable for cashier happy path + failure scenarios

### KPI Targets

- Design signoff complete
- 0 critical ambiguity in core cashier flow

---

## Sprint 2 — Catalog & Menu Management Core

### Backlog Items

- Category/product CRUD with statuses and ordering
- Modifier groups, toppings, combo definitions with pricing rules
- Menu publish/unpublish by time slot
- Import template for product/menu CSV

### Acceptance Criteria

- Create/update/archive category/product/modifier/combo works end-to-end
- Rule validation prevents invalid combo/modifier setup
- Import template validates and returns row-level errors

### KPI Targets

- Menu setup for 500 SKUs in < 90 minutes
- Validation errors are actionable in > 95% test cases

---

## Sprint 3 — Cashier Order Flow v1

### Backlog Items

- New order, add/remove items, modifier selection, notes
- Table map basic states (empty/occupied/paid)
- Hold/resume order
- Kitchen/bar ticket print trigger

### Acceptance Criteria

- Cashier can complete order creation under normal and degraded network
- Printed kitchen ticket includes modifiers/notes correctly
- Hold/resume preserves state without data loss

### KPI Targets

- Median add-item interaction <= 2 taps for popular items
- Print success >= 98% in test lab

---

## Sprint 4 — Billing & Payments v1

### Backlog Items

- Split bill by item/quantity/custom amount
- Merge bills, transfer table/order
- Payment methods: cash, bank QR, e-wallet (provider abstraction)
- Receipt print and reprint with permission checks

### Acceptance Criteria

- Split/merge/transfer produce consistent final totals and tax/service charges
- Payment finalization writes immutable transaction records
- Reprint requires authorized role and logs audit trail

### KPI Targets

- Order-to-payment <= 75 seconds median (pilot script)
- Payment completion success >= 99%

---

## Sprint 5 — Reporting Core + Shift Management

### Backlog Items

- Real-time revenue dashboard (store/day/shift)
- Shift open/close, cash count, variance capture
- Top-selling and slow-selling items
- Export daily summary CSV/PDF

### Acceptance Criteria

- Dashboard updates within acceptable lag (< 60s near real-time)
- Shift close report can be generated with role checks
- Top/slow ranking configurable by period

### KPI Targets

- P95 dashboard load <= 3.0s
- Shift close report generated in <= 10s

---

## Sprint 6 — Authorization Hardening & Auditability

### Backlog Items

- Fine-grained action permissions (edit item, void, delete, reprint, report)
- Supervisor approval workflows for risky actions
- Immutable audit log with actor, action, before/after snapshot refs
- Alerting on suspicious operations pattern

### Acceptance Criteria

- Unauthorized actions blocked at both UI and API layers
- Approval-required actions cannot bypass workflow
- Audit trail export supports compliance review

### KPI Targets

- 100% critical actions audited
- 0 high-severity auth bypass in QA penetration tests

---

## Sprint 7 — E-Invoice + Digital Signature Integration v1

### Backlog Items

- Integrate e-invoice provider adapter
- Invoice issuance at checkout and post-payment fallback queue
- Digital signing flow and status tracking
- Invoice search, filter, and resend

### Acceptance Criteria

- Invoice can be issued/signed/sent from mobile cashier flow
- Failed issuance enters retry queue with clear operator status
- Search retrieves invoice by code, customer, time, amount

### KPI Targets

- Successful e-invoice issuance >= 97% first attempt
- Retry success within 15 min >= 99%

---

## Sprint 8 — Offline Reliability & Conflict Resolution

### Backlog Items

- Robust offline queue (priority channels for payment/invoice/print)
- Conflict strategies (last-write-wins only where safe, otherwise supervisor resolve)
- Sync health dashboard for operators/admin
- Chaos testing for network flaps and device restarts

### Acceptance Criteria

- No data loss across forced offline/online transitions
- Conflicts surfaced with actionable resolution paths
- Sync health indicates lag, failures, and queue depth

### KPI Targets

- Sync success after reconnect <= 5 min for 99% operations
- Data integrity mismatch rate < 0.1%

---

## Sprint 9 — Inventory Lite v1

### Backlog Items

- Recipe-to-ingredient mapping (BOM-lite)
- Auto deduction from completed sales
- Stock in/out/adjustment workflows
- Low-stock and out-of-stock alerts

### Acceptance Criteria

- Sold quantity deducts ingredients accurately per recipe
- Stock adjustment requires role + reason code
- Alert thresholds configurable per item/store

### KPI Targets

- Stock deduction accuracy >= 98% in controlled pilot
- Inventory event latency <= 60s from sale completion

---

## Sprint 10 — Multi-Branch Reporting + Owner View

### Backlog Items

- Branch-level and aggregated chain dashboards
- Compare stores by revenue, ticket size, labor proxy metrics
- Owner-focused weekly digest report
- Access control by branch scope

### Acceptance Criteria

- Multi-branch KPIs are filterable by period and branch group
- Branch-scoped users cannot access unauthorized data
- Weekly digest auto-generated and downloadable

### KPI Targets

- P95 multi-branch report load <= 3.5s
- 100% branch access control test pass

---

## Sprint 11 — Migration/Onboarding Tooling + Support Readiness

### Backlog Items

- Guided onboarding wizard (business profile, tax, menu, users, printers)
- Migration import from competitor formats (CSV baseline)
- In-app diagnostics page (printer, network, sync, provider status)
- Support playbook + escalation matrix + SLA response templates

### Acceptance Criteria

- New store can reach first live sale within <= 2 hours
- Migration tool highlights incompatible fields with fix guidance
- Diagnostics produce downloadable support bundle

### KPI Targets

- Onboarding completion <= 120 minutes for 80% new stores
- First-week support tickets/store <= 5

---

## Sprint 12 — Pilot Launch, Pricing Experiment, GTM Readiness

### Backlog Items

- Pilot rollout to 10–20 stores with monitoring war-room
- Pricing plan A/B (subscription tiers + add-ons)
- Case-study metrics collection and testimonial process
- Final release hardening and go/no-go checklist

### Acceptance Criteria

- Pilot stores run full business day with no critical blockers
- Pricing experiment data sufficient for packaging decision
- Launch checklist complete with incident runbook

### KPI Targets

- Pilot retention after 30 days >= 85%
- Critical incident rate < 1 per store-month

## 5) Definition of Done (Cross-Sprint)

- Functional: feature passes acceptance criteria and UAT scenarios
- Non-functional: performance/security/reliability benchmarks met
- Observability: dashboards/alerts/logs in place for the new flow
- Documentation: operator guide + troubleshooting + release notes
- Support readiness: known-issues, workaround, escalation owner assigned

## 6) Key Risks and Mitigation

- Offline data inconsistency risk
  - Mitigation: idempotency keys, conflict-safe domain rules, replay tests
- Hardware fragmentation (printer/POS devices)
  - Mitigation: capability matrix, certified device list, adapter pattern
- E-invoice regulatory/provider changes
  - Mitigation: provider abstraction layer, monitoring, fallback queue + manual retry
- Scope creep (trying to match all iPOS modules too early)
  - Mitigation: strict release gates, defer HRM and deep analytics to post-pilot

## 7) Platform Support Matrix (Recommendation)

### Must Have in 6 Months

- Android POS terminal (cashier full)
- Android phone/tablet (cashier lite/full by profile)
- Web admin on desktop browsers (Chrome/Edge)

### Should Have Next Phase

- iOS app for manager/owner monitoring + invoice/report actions
- Kitchen display mode app

### Later (Demand-Driven)

- Dedicated Windows POS shell (enterprise-heavy customers)

## 8) Competitive Positioning Guidance

- Win on execution quality, not feature count: speed, stability, and support
- Promise and prove: “Works offline reliably, syncs safely, closes shifts correctly”
- Differentiate with practical onboarding + migration + responsive local support
- Use pilot metrics as sales proof instead of broad marketing claims

## 9) Suggested Immediate Next Actions (Next 2 Weeks)

- Lock target segment and pricing hypothesis
- Freeze MVP scope for Sprints 1-4
- Confirm integration partners (QR payment, e-invoice, printers)
- Run discovery interviews with 8-12 stores (current iPOS users + switchers)
- Build sprint 1 backlog in issue tracker with owners and estimates

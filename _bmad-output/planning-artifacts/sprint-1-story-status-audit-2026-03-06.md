# Sprint 1 Story Status Audit (2026-03-06)

Source backlog: [_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md](_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md)

## Summary

- Done: 5/11
- Partial: 2/11
- Not done: 4/11

## Story-by-story status

### S1-01 — Define bounded contexts and ownership
- Status: Not done
- Evidence found:
  - Architecture has general structure/decisions but no explicit bounded-context ownership matrix and no context map artifact: [_bmad-output/planning-artifacts/architecture.md](_bmad-output/planning-artifacts/architecture.md)
- Needed to complete:
  - Create explicit context map + owner per context (Catalog, Order, Table, Payment, Invoice, Inventory-lite)
  - Add cross-context event list + versioning

### S1-02 — ADRs for sync, payments, printing
- Status: Partial
- Evidence found:
  - Sync protocol draft exists: [_bmad-output/implementation-artifacts/sync-protocol.md](_bmad-output/implementation-artifacts/sync-protocol.md)
  - Sync envelope draft exists: [_bmad-output/implementation-artifacts/tenancy/sync-envelope.md](_bmad-output/implementation-artifacts/tenancy/sync-envelope.md)
- Gaps:
  - No ADR-001/002/003 files with alternatives/trade-offs
  - No dedicated payment abstraction ADR
  - No printer abstraction/fallback ADR

### S1-03 — Role-permission matrix
- Status: Done
- Evidence found:
  - Role-permission matrix document created with allow/deny/approval-required mappings: [_bmad-output/planning-artifacts/role-permission-matrix.md](_bmad-output/planning-artifacts/role-permission-matrix.md)
  - UI behaviors and API policy names specified.
- Gaps:
  - None

### S1-04 — Policy skeleton implementation (API + UI)
- Status: Done
- Evidence found:
  - AddAuthorization/UseAuthorization setup implemented in `src/GoiMon.Api/Program.cs`.
  - Roles, policy constants, policy matrix wired in `src/GoiMon.Api/Infrastructure/Authorization/AuthorizationConfig.cs`.
  - Role-action enforcement tests added in `tests/GoiMon.Api.Tests/AuthorizationPolicyTests.cs`.
  - UI guard pattern created in `_bmad-output/implementation-artifacts/authorization-ui-guard.md`.
- Gaps:
  - None

### S1-05 — Sync event schema and queue contract
- Status: Partial
- Evidence found:
  - Sync schema draft/spec present: [_bmad-output/implementation-artifacts/sync-protocol.md](_bmad-output/implementation-artifacts/sync-protocol.md)
  - Envelope contract present: [_bmad-output/implementation-artifacts/tenancy/sync-envelope.md](_bmad-output/implementation-artifacts/tenancy/sync-envelope.md)
  - Outbox processing exists with retry attempts: [src/GoiMon.Api/Infrastructure/Outbox/OutboxService.cs](src/GoiMon.Api/Infrastructure/Outbox/OutboxService.cs)
- Gaps:
  - No explicit queue state model (pending/sent/acked/failed/dead-letter)
  - No published contract tests for schema + idempotent replay

### S1-06 — Sync simulator + failure scenarios
- Status: Not done
- Evidence found:
  - No simulator artifact in tests or tooling folders
  - Existing tests are unrelated to sync simulator: [tests/GoiMon.Staff.Tests/Features/Authentication/TokenStorageServiceTests.cs](tests/GoiMon.Staff.Tests/Features/Authentication/TokenStorageServiceTests.cs)
- Gaps:
  - No automated offline/reconnect/duplicate/out-of-order scenarios
  - No CI artifact/report for sync failure matrix

### S1-07 — Telemetry + dashboard baseline
- Status: Done
- Evidence found:
  - `OrderTelemetry.cs` — orders channel counters (created, validation_failed, selected_modifiers).
  - `PosOperationTelemetry.cs` — NEW: sync, payment, print channels wired via `IPosOperationTelemetry`.
  - `OutboxService.cs` — instrumented with `TrackSyncEventProcessed`, `TrackSyncEventFailed`, `TrackSyncDeadLettered`.
  - Metric catalog standardized and documented: `_bmad-output/implementation-artifacts/telemetry-metric-catalog.md`.
  - Seq alert filter expressions documented per channel, ready for ops team to configure.
- Gaps:
  - None (payment/print instrumented at scaffold level; full instrumentation when adapters are built in Sprint 2).

### S1-08 — Correlation IDs and traceability standard
- Status: Done
- Evidence found:
  - `CorrelationIdMiddleware.cs` — implemented and registered in `Program.cs`.
  - `CorrelationIdMiddlewareTests.cs` — unit tests for ID generation and propagation.
  - `tracing-query-cheatsheet.md` — support artifact for tracing in Seq.
- Gaps:
  - None.

### S1-09 — Cashier flow clickable prototype
- Status: Done
- Evidence found:
  - Interactive prototype implemented at `/prototype/cashier`.
  - Glassmorphic `PrototypeLayout.razor` with `ConnectionStatus` toggle.
  - `TableGridView.razor` and `OrderingView.razor` provide full interactive flow.
- Gaps:
  - None (Prototypes meet ACs).

### S1-10 — UX handoff package for Sprint 2
- Status: Not done
- Evidence found:
  - No dedicated UX handoff artifact found under planning/implementation outputs
- Gaps:
  - Missing components/states/interaction handoff package and engineering signoff

### S1-11 — Sprint gate checklist and readiness baseline
- Status: Not done
- Evidence found:
  - Exit criteria exist in backlog doc only: [_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md](_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md)
- Gaps:
  - No dedicated go/no-go checklist artifact with evidence mapping

## Immediate priority to close Sprint 1

1. Create three ADR documents (sync, payment abstraction, printer abstraction) and sign off.
2. Create role-permission matrix doc and wire minimum authorization skeleton in API.
3. Formalize sync contract test pack + small simulator scenarios.
4. Publish telemetry metric catalog + alert thresholds.
5. Produce UX prototype/handoff artifacts and Sprint 1 go/no-go checklist.

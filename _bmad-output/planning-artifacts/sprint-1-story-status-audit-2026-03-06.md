# Sprint 1 Story Status Audit (2026-03-06)

Source backlog: [_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md](_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md)

## Summary

- Done: 2/11
- Partial: 4/11
- Not done: 5/11

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
  - Existing tests are unrelated to sync simulator: [tests/GoiMon.Client.Tests/Features/Authentication/TokenStorageServiceTests.cs](tests/GoiMon.Client.Tests/Features/Authentication/TokenStorageServiceTests.cs)
- Gaps:
  - No automated offline/reconnect/duplicate/out-of-order scenarios
  - No CI artifact/report for sync failure matrix

### S1-07 — Telemetry + dashboard baseline
- Status: Partial
- Evidence found:
  - Order telemetry counters/logging implemented: [src/GoiMon.Api/Features/Orders/Services/OrderTelemetry.cs](src/GoiMon.Api/Features/Orders/Services/OrderTelemetry.cs)
  - Serilog configured and running: [src/GoiMon.Api/Program.cs](src/GoiMon.Api/Program.cs), [src/GoiMon.Api/appsettings.json](src/GoiMon.Api/appsettings.json)
  - Hangfire dashboard exists (job dashboard, not product ops dashboard): [src/GoiMon.Api/Program.cs](src/GoiMon.Api/Program.cs)
- Gaps:
  - Missing sync/print/payment unified dashboard panels and alert rules
  - Missing documented metric catalog for Sprint 1 acceptance

### S1-08 — Correlation IDs and traceability standard
- Status: Partial
- Evidence found:
  - Correlation concept documented in architecture/sync docs: [_bmad-output/planning-artifacts/architecture.md](_bmad-output/planning-artifacts/architecture.md), [_bmad-output/implementation-artifacts/sync-protocol.md](_bmad-output/implementation-artifacts/sync-protocol.md)
- Gaps:
  - No middleware/implementation proving end-to-end correlation propagation
  - No support query cheatsheet artifact for tracing

### S1-09 — Cashier flow clickable prototype
- Status: Not done
- Evidence found:
  - Planning expectations exist only: [_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md](_bmad-output/planning-artifacts/goimon-sprint-1-ticket-breakdown.md)
- Gaps:
  - No prototype deliverable file/link and no approval evidence

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

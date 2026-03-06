# 🔗 Dev Story: Correlation IDs and Traceability Standard

**Status:** Done  
**Date Created:** 2026-03-07  
**Owner:** Dev Agent  
**User:** Chicuong  
**Story Key:** S1-08-correlation-ids

---

## Story

As a **support engineer**,  
I want **trace IDs propagated from UI through to backend events and adapters**,  
so that **incidents can be diagnosed quickly by following a single correlation ID chain**.

---

## Scope

### In Scope
- Correlation ID middleware for ASP.NET Core (auto-generate or accept incoming `X-Correlation-Id` header)
- Propagation of Correlation ID into Serilog log context (all log entries carry `CorrelationId`)
- Propagation of Correlation ID into outbox event payloads (sync events carry the ID)
- Structured logging format standard documented
- Seq query cheat-sheet for support team

### Out of Scope
- Client (Blazor) side propagation — done when client stories are implemented in Sprint 2
- Distributed trace integration with OpenTelemetry spans — future sprint

---

## Dependencies

- S1-07 — Telemetry baseline done (Serilog + Seq running)
- `GoiMon.Api.Infrastructure.Telemetry.PosOperationTelemetry` available

---

## Acceptance Criteria

- [x] **AC1**: Every HTTP request gets a `CorrelationId` — generated if absent, accepted if sent via `X-Correlation-Id` header
- [x] **AC2**: `CorrelationId` appears in every Serilog log entry for that request (via `LogContext.PushProperty`)
- [x] **AC3**: `CorrelationId` is returned in the HTTP response header `X-Correlation-Id`
- [x] **AC4**: The logging query cheat-sheet artifact exists at `_bmad-output/implementation-artifacts/tracing-query-cheatsheet.md`

---

## Task Breakdown

### TASK 1 — Correlation ID Middleware (AC1, AC2, AC3)
- [x] Create `CorrelationIdMiddleware` that reads/generates correlation ID and pushes it to `ILogger` scope and Serilog `LogContext`
- [x] Register middleware in `Program.cs` before `UseAuthentication`
- [x] Ensure response header `X-Correlation-Id` is set

**Files:**
- `src/GoiMon.Api/Infrastructure/Middleware/CorrelationIdMiddleware.cs`
- `src/GoiMon.Api/Program.cs`

### TASK 2 — Tests (AC1, AC2, AC3)
- [x] Unit test: middleware generates ID when header absent
- [x] Unit test: middleware uses existing ID when header present
- [x] Unit test: response header is set correctly

**Files:**
- `tests/GoiMon.Api.Tests/CorrelationIdMiddlewareTests.cs`

### TASK 3 — Tracing Query Cheat-Sheet Artifact (AC4)
- [x] Create `tracing-query-cheatsheet.md` with Seq filter recipes for correlation ID tracing

**Files:**
- `_bmad-output/implementation-artifacts/tracing-query-cheatsheet.md`

---

## Verification Plan

- [ ] Build: `dotnet build src/GoiMon.Api`
- [ ] Tests: `dotnet test tests/GoiMon.Api.Tests`
- [ ] Manual: send a request to `/health` with `X-Correlation-Id: test-123`, confirm response header echoes `test-123`
- [ ] Manual: send a request without the header, confirm auto-generated UUID appears in response

---

## Definition of Ready (DoR)

- [x] Story key is unique
- [x] Scope in/out is explicit
- [x] Acceptance criteria are measurable
- [x] Dependencies are listed
- [x] Impacted files/layers are identified

---

## Definition of Done (DoD)

- [ ] All ACs completed
- [ ] Build passes on impacted projects
- [ ] Unit tests pass
- [ ] Status updated to Done in audit file

---

## Dev Notes

### Design Decisions
1. Use `ILogger` scope + Serilog `LogContext.PushProperty` so the correlation ID flows through ALL log calls in the request scope without callers needing to pass it explicitly.
2. Accept `X-Correlation-Id` as the canonical header name (widely used in REST APIs and AWS).
3. Correlation ID format: UUID v4 (`Guid.NewGuid().ToString()`).

### Risks
- None — purely additive infrastructure change.

---

## Dev Agent Record

### Implementation Plan
- TASK 1: Write `CorrelationIdMiddleware` → register → tests → cheat-sheet
- All tasks in single execution pass

### Completion Notes
_To be filled during implementation_

---

## File List

_Updated after each task_

---

## Change Log

- 2026-03-07 — Story created, status set to In Progress by Dev Agent

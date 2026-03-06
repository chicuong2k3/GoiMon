# GoiMon Sprint 1 Ticket Breakdown

## Scope Reference

This sprint plan is derived from Sprint 1 in the main roadmap:
- Product/Architecture Foundation
- Bounded contexts, role matrix, offline sync contract, observability baseline, cashier UX flows

## Sprint Goal

Establish a production-ready foundation for POS MVP by closing architecture ambiguity, defining role-based behavior, and proving offline-first operational design before feature-heavy implementation begins.

## Capacity and Estimation Model

- Sprint length: 2 weeks
- Team assumption: 3 engineers, 1 UX, 1 PM/BA, 1 QA (shared)
- Estimation scale: Fibonacci (1, 2, 3, 5, 8, 13)
- Total target effort: 58-66 points

## Sprint 1 Backlog (Stories + Tasks)

### EPIC S1-E1: Domain and Architecture Foundation

### Story S1-01: Define bounded contexts and ownership
- Status: **Done** (Artifacts created: `bounded-contexts.md`, `context-map.md`. **Action**: Renamed `GoiMon.Client` to `GoiMon.Staff`)
- Story points: 5
- Owner: PM/BA + Tech Lead
- User story: As a product and engineering team, we need clear domain boundaries so we can build independently without conflicting models.
- Scope:
  - Define contexts: Catalog, Order, Table, Payment, Invoice, Inventory-lite
  - Define aggregate roots, key invariants, and ownership boundaries
  - Produce context map and data flow diagram
- Acceptance criteria:
  - Each context has explicit owner and integration boundary
  - Cross-context events are named and versioned
  - Context map reviewed and approved by engineering + PM
- Dependencies: none
- Test cases:
  - Review checklist confirms no duplicated ownership between contexts
  - Review checklist confirms each shared concept has a system of record

### Story S1-02: Architecture decisions for sync, payments, and printing
- Status: **Done** (Artifacts created: `ADR-001`, `ADR-002`, `ADR-003` in `implementation-artifacts/adrs/`)
- Story points: 8
- Owner: Tech Lead + Senior Engineer
- User story: As an implementation team, we need explicit architecture decisions to avoid rework in Sprints 2-4.
- Scope:
  - ADR-001 Offline queue + idempotency strategy
  - ADR-002 Payment abstraction and provider adapter contract
  - ADR-003 Printer abstraction and fallback behavior
- Acceptance criteria:
  - 3 ADRs approved with alternatives and trade-offs documented
  - Non-functional constraints listed (latency, reliability, recoverability)
  - Risks and mitigation mapped per ADR
- Dependencies: S1-01
- Test cases:
  - ADR quality checklist passes for all 3 ADRs
  - At least 2 alternatives evaluated in each ADR

### EPIC S1-E2: Authorization and Compliance Foundation

### Story S1-03: Define role-permission matrix for critical POS actions

- Status: **Done** (Artifact created: `role-permission-matrix.md`)
- Story points: 8
- Owner: PM/BA + Engineer
- User story: As a business owner, I need role-based control so risky operations are restricted and auditable.
- Scope:
  - Roles: Cashier, Supervisor, Manager, Owner, Accountant
  - Actions: order edit/void/delete, reprint invoice, report access, shift close, stock adjustment
  - Approval-required actions matrix
- Acceptance criteria:
  - Matrix includes allow/deny/approval-required for each role-action pair
  - Matrix maps to UI behavior and API policy names
  - Signoff by PM + Tech Lead
- Dependencies: S1-01
- Test cases:
  - 100% action list has mapped policy name
  - No conflicting rules across roles for same action

### Story S1-04: Policy skeleton implementation (API + UI guard)

- Status: **Done** (Artifacts created: `AuthorizationConfig.cs`, `authorization-ui-guard.md`)
- Story points: 5
- Owner: Engineers
- User story: As a developer team, we need a policy skeleton to enforce permissions consistently from Sprint 2 onward.
- Scope:
  - Add policy constants and middleware hooks
  - Add UI guard components/patterns
  - Add audit event placeholders for restricted actions
- Acceptance criteria:
  - Unauthorized actions blocked in API integration tests
  - UI hides/locks prohibited actions by role
  - Guarding approach documented for future stories
- Dependencies: S1-03
- Test cases:
  - API test: Cashier cannot access manager-only endpoint
  - UI test: Restricted action button not actionable for unauthorized role

### EPIC S1-E3: Offline-First Sync Contract

### Story S1-05: Define sync event schema and queue contract

- Story points: 8
- Owner: Engineers
- User story: As a system, I need deterministic sync messages so offline operations can replay safely.
- Scope:
  - Event envelope: eventId, aggregateId, version, tenant/store, timestamp, actor, idempotencyKey
  - Queue states: pending, sent, acked, failed, dead-letter
  - Retry policy and backoff rules
- Acceptance criteria:
  - Versioned schema documented with sample payloads
  - Queue state transitions and retry limits documented
  - Conflict categories defined (safe auto-resolve vs manual review)
- Dependencies: S1-02
- Test cases:
  - Contract test validates required fields and schema version
  - Replay test shows duplicate events are idempotent

- Status: Done — see ADR: [_bmad-output/implementation-artifacts/adrs/ADR-005-sync-event-schema.md](_bmad-output/implementation-artifacts/adrs/ADR-005-sync-event-schema.md)

### Story S1-06: Build sync simulator and failure scenarios

- Story points: 5
- Owner: Engineers + QA
- User story: As QA and engineering, we need simulation tools to validate reconnect and retry behavior before production workloads.
- Scope:
  - Simulate offline, reconnect, duplicate events, out-of-order delivery
  - Export scenario report with pass/fail outcomes
- Acceptance criteria:
  - At least 5 failure scenarios automated
  - Simulator report available in CI artifacts
- Dependencies: S1-05
- Test cases:
  - Scenario: network flap every 10s for 2 minutes
  - Scenario: duplicate event delivered 3 times
  - Scenario: out-of-order event application

### EPIC S1-E4: Observability Baseline

### Story S1-07: Telemetry and baseline dashboard instrumentation

- Story points: 5
- Owner: Engineers
- User story: As operations/support, we need visibility into sync, print, and cashier failures to reduce incident recovery time.
- Scope:
  - Instrument event counters: sync queue depth, sync failures, print failures, payment errors
  - Add dashboard panels and alert thresholds
- Acceptance criteria:
  - Dashboard displays real-time key counters in non-prod
  - Alert rules fire for configured thresholds
  - Metric names standardized and documented
- Dependencies: S1-02, S1-05
- Test cases:
  - Synthetic error injection increments expected metric
  - Alert test validates notification path

### Story S1-08: Correlation IDs and traceability standard

- Story points: 3
- Owner: Engineers
- User story: As support, I need trace IDs from UI to backend so incidents can be diagnosed quickly.
- Scope:
  - Correlation ID propagation standard for request -> event -> external adapter
  - Logging format standard and sample query recipes
- Acceptance criteria:
  - One transaction traceable across all major services
  - Logging query cheat sheet available for support team
- Dependencies: S1-07
- Test cases:
  - Trace test: one cashier action yields consistent correlation ID chain

### EPIC S1-E5: UX Foundation for Cashier Flow

### Story S1-09: Cashier flow prototype (happy path + degraded path)

- Story points: 8
- Owner: UX + PM/BA
- User story: As a cashier, I need a clear and fast workflow so service speed remains high even under stress.
- Scope:
  - Prototype flows: order entry, split bill, checkout, error/retry
  - Include offline indicators and retry prompts
- Acceptance criteria:
  - Clickable prototype approved by PM + Tech Lead + one cashier representative
  - Edge states represented for network failure and retry
- Dependencies: S1-03, S1-05
- Test cases:
  - Usability walk-through with 5 scripted tasks
  - Task completion rate >= 90% in internal review

### Story S1-10: UX spec handoff package for Sprint 2 build

- Story points: 3
- Owner: UX
- User story: As engineers, we need complete UX specs to implement without ambiguity in Sprint 2.
- Scope:
  - Components, spacing, states, empty/error/loading behavior
  - Interaction notes and keyboard/touch targets
- Acceptance criteria:
  - Handoff package includes all required states and assets
  - Zero blocker questions from engineering handoff review
- Dependencies: S1-09
- Test cases:
  - Handoff checklist complete
  - Engineering signoff captured

### EPIC S1-E6: Readiness and Governance

### Story S1-11: Sprint gate checklist and release readiness baseline

- Story points: 3
- Owner: PM/BA + QA
- User story: As a delivery team, we need objective quality gates to decide if Sprint 1 outputs are truly ready.
- Scope:
  - Define DoR/DoD for architecture and contract artifacts
  - Define go/no-go checklist for entering Sprint 2
- Acceptance criteria:
  - Checklist approved and used in sprint review
  - All Sprint 1 deliverables mapped to checklist evidence
- Dependencies: S1-01 to S1-10
- Test cases:
  - Checklist dry run on one completed artifact

## Dependency Map (Execution Order)

1. S1-01 -> S1-02 -> S1-05 -> S1-06
2. S1-01 -> S1-03 -> S1-04
3. S1-02 + S1-05 -> S1-07 -> S1-08
4. S1-03 + S1-05 -> S1-09 -> S1-10
5. S1-01..S1-10 -> S1-11

## Suggested Assignment by Role

- PM/BA: S1-01, S1-03, S1-11
- UX: S1-09, S1-10
- Engineer A: S1-02, S1-05
- Engineer B: S1-04, S1-07
- Engineer C: S1-06, S1-08
- QA (shared): S1-06, S1-11, support on policy and contract tests

## Sprint 1 Test Plan Summary

### Functional

- Permission enforcement in UI/API for critical actions
- Sync contract validity and idempotent replay behavior

### Non-Functional

- Baseline latency logging and error-rate telemetry active
- Resilience scenarios for network flap and duplicate delivery

### UAT/Workflow

- Cashier prototype task-based review
- Approval from PM, tech lead, and pilot cashier representative

## Exit Criteria for Sprint 1

- All acceptance criteria for S1-01 to S1-11 are met
- Architecture and sync contract have no unresolved critical risk
- Role matrix is implementation-ready for Sprint 2 feature stories
- Observability baseline can detect failures in sync/print/payment channels
- UX package is complete for Sprint 2 development

## Risks to Watch During Sprint 1

- Underestimating sync complexity and conflict taxonomy
- Delayed decision on provider adapter contracts
- Incomplete role-action matrix causing rework in Sprint 2

## Immediate Next Actions (Next 48 Hours)

- Run Sprint 1 planning poker for S1-01 to S1-11
- Confirm owners and due dates per story
- Create issues in tracker with labels: epic:S1, sprint:1, priority
- Schedule architecture review and UX review ceremonies

---
title: "Architecture Decisions - GoiMon"
project_name: GoiMon
createdBy: Chicuong
date: 2026-02-26T00:00:00Z
stepsCompleted:
  - 1
  - 2
  - 3
  - 4
  - 5
  - 6
  - 7
  - 8
inputDocuments:
  - path: _bmad-output/planning-artifacts/prd.md
    description: Product Requirements Document (PRD)
  - path: _bmad-output/planning-artifacts/product-brief-GoiMon-2026-02-26.md
    description: Product Brief
  - path: _bmad-output/project-context.md
    description: Project context (empty)
workflowType: architecture
lastStep: 8
status: 'complete'
completedAt: 2026-02-26
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

---

## Loaded Input Documents

- PRD: _bmad-output/planning-artifacts/prd.md
- Product Brief: _bmad-output/planning-artifacts/product-brief-GoiMon-2026-02-26.md
- Project Context: _bmad-output/project-context.md (empty)

---

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**
- Staff Order UI: large-button staff UI supporting modifiers, combos, and instant totals.
- Menu Management with CSV import for quick setup.
- Offline-first local store with queued operations and conflict-resilient sync.
- Customer Self-Order via QR leading to a lightweight menu page.
- Cash-first payment handling with optional QR/mobile-pay hooks.
- Device pairing and basic multi-device sync.
- Guided onboarding flow to set up in ≤10 minutes; daily summary/export for reconciliation.

**Non-Functional Requirements:**
- Offline reliability (≥99% local order persistence).
- Sync reliability (≥98% when connectivity returns).
- Support for low-end and legacy devices (iOS fallback required).
- Fast order entry performance and small asset footprint.
- Simple security model (PIN or device-bound session) for staff actions.

### Scale & Complexity

- Complexity level: medium (offline-first + multi-device sync + legacy device support).
- Primary technical domain: Web (Blazor WASM) + mobile/browser fallback + backend GraphQL API.
- Estimated architectural components: Frontend (WASM + fallback), local persistence & service worker, sync/gateway layer, GraphQL API, auth/payment hooks, reconciliation/export subsystem.

### Technical Constraints & Dependencies

- Blazor WASM compatibility risks on legacy iOS; static fallback required.
- Use `EasyAppDev.Blazor.Store` for local state and queued operations.
- Require a Service Worker for asset caching and offline load.
- Optional local gateway (Raspberry Pi) or LAN sync for venues without internet.
- Minimal external payment integrations in MVP (QR hooks only).

### Cross-Cutting Concerns Identified

- Offline sync conflict resolution and strong data integrity for reconciliation.
- UX constraints for low-technical-skill users (large fonts, simple flows).
- Security around reconciliation and staff actions (PIN/device binding).
- Performance and compatibility across older devices; graceful degradation.

---

Appended project context analysis based on loaded input documents.

Next: choose one of the collaboration menus in step-02 (A/P/C). The workflow will continue to step-03 after you select C.

## Starter Template Evaluation

### Primary Technology Domain

Based on the PRD and product brief, the primary technology domain is a Web application using Blazor WebAssembly (WASM) for the client and a .NET-based backend exposing a GraphQL API.

### Starter Options Considered

1) Blazor WebAssembly Hosted template (official `dotnet new blazorwasm --hosted`)
- Rationale: provides a single solution containing client (WASM), server (ASP.NET Core) and shared projects — reduces integration friction and is well-supported in .NET.

2) Blazor WebAssembly PWA-enabled template (`dotnet new blazorwasm --pwa`)
- Rationale: builds a Progressive Web App with service worker support for offline caching; good match for offline-first requirements.

3) Hot Chocolate (ChilliCream) GraphQL server added to an ASP.NET Core starter
- Rationale: Hot Chocolate is the leading GraphQL server for .NET; easy to add to an ASP.NET Core backend using `dotnet add package HotChocolate.AspNetCore` and `builder.Services.AddGraphQLServer()`.

### Recommended Starter Approach

- Use the official Blazor WebAssembly Hosted template as the base to get client+server scaffolding, and enable PWA features for the client to meet offline caching requirements.
- For the GraphQL API, add Hot Chocolate to the server project (well-documented and actively maintained). This gives a clear separation: Blazor WASM client (PWA, local store), ASP.NET Core server hosting GraphQL and sync endpoints.

### Initialization Commands (examples)

```bash
# Create hosted Blazor WASM solution
dotnet new blazorwasm --hosted -o GoiMonApp

# Inside the server project, add Hot Chocolate
cd GoiMonApp/Server
dotnet add package HotChocolate.AspNetCore
```

Note: Verify the latest `dotnet` and Hot Chocolate versions before running; official docs were checked for current guidance (Blazor docs and Hot Chocolate get-started guide).

### Architectural Decisions Provided by This Starter

- Language & Runtime: C# / .NET (recommended .NET 8+ for widest compatibility and long-term support). Verify latest LTS at runtime.
- Styling & Client: Blazor components; choose a lightweight CSS framework (Tailwind or BlazorBlueprint) to keep asset size small for low-end devices.
- Offline & PWA: Service Worker + caching from PWA template; use `EasyAppDev.Blazor.Store` for local persistence and queued sync.
- API: GraphQL server powered by Hot Chocolate for flexible queries; dedicated sync endpoints can be added for queued operations.
- Auth: Simple device-bound sessions / PIN flow implemented server-side with minimal token lifetime and local device binding logic.
- Dev Experience: `dotnet` CLI templates give hot reload in development; recommend Visual Studio Code launch tasks for iteration.

---

Appended starter template evaluation. Reply with [A] Advanced Elicitation, [P] Party Mode, or [C] Continue to save and proceed to step-04.

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Database selection and data model for offline-first queued orders and reconciliation.
- Sync protocol and conflict-resolution strategy for queued operations.

**Important Decisions (Shape Architecture):**
- Authentication model (device-bound PIN sessions) and minimal token lifecycle.
- API pattern: GraphQL for flexible client queries plus REST-like sync endpoints for queued operations.
- Frontend state management and offline persistence strategy.

**Deferred Decisions (Post-MVP):**
- Full payment processor integrations and multi-tenant billing model.

---

### Data Architecture

- Database choice: Recommend PostgreSQL for server-side relational storage (durable transactions, reliable reconciliation, mature tooling). Use a lightweight embedded store (IndexedDB via `EasyAppDev.Blazor.Store`) on the client for local persistence.
- Data modeling: Design `Orders`, `OrderItems`, `MenuItems`, `Devices`, and `SyncQueue` entities. Keep server canonical; client stores minimal denormalized copies for fast UI.
- Migrations: Use EF Core migrations on the server for schema versioning.
- Caching: Server-side caching (Redis) can be introduced later for read-heavy endpoints; not required for MVP.

### Authentication & Security

- Auth: Device-bound session tokens + optional PIN; tokens short-lived and refreshable via device pairing flow. Sensitive operations (reconciliation, payouts) require stronger verification.
- Authorization: Role-lite model (owner, staff, temp) enforced server-side.
- Data encryption: Use TLS in transit; at-rest encryption handled by hosting provider for managed DBs; client-side data in IndexedDB encrypted if required by local regulations.

### API & Communication

- Use GraphQL (Hot Chocolate) for primary client-server data operations and queries.
- Provide dedicated REST-like endpoints for sync mechanics (batch upload of queued operations, conflict resolution callbacks) to simplify idempotency and retries.
- Error handling: standardize error codes and use operation IDs for idempotency.

### Frontend Architecture

- State management: `EasyAppDev.Blazor.Store` for local persistence and queued operations; central app store pattern for UI state.
- Component patterns: Small, accessible components (large tappable areas); separate presentation and container components.
- Routing & performance: PWA + service worker for asset caching; code-splitting and minimize payloads to support low-end devices.

### Infrastructure & Deployment

- Hosting: Lightweight cloud (DigitalOcean, Railway) or self-hosted VM for server; PostgreSQL managed or self-hosted. Consider Raspberry Pi local gateway option for offline-first venues.
- CI/CD: Use GitHub Actions for build/test/deploy pipelines; include `dotnet test` and container image builds for server.
- Monitoring: Basic logging (structured JSON) and error reporting (Sentry or equivalent) for MVP.

### Decision Impact Analysis

- Implementation sequence:
  1. Scaffold Blazor WASM hosted solution + PWA features
  2. Add Hot Chocolate and implement basic schema for Menu and Orders
  3. Implement client IndexedDB persistence + `SyncQueue` upload endpoint
  4. Add auth/pairing flow and secure token handling
  5. Implement reconciliation and daily summary export

---

Appended core architectural decisions. Reply with [A] Advanced Elicitation, [P] Party Mode, or [C] Continue to save and proceed to step-05.

## Implementation Patterns & Consistency Rules

### Critical Conflict Points Identified

- Naming conventions (DB, API, code, files)
- Data format differences (camelCase vs snake_case)
- Sync/idempotency and error handling for queued operations
- Component and folder organization across teams/agents
- Event and action naming in client-server communication

### Naming Patterns

**Database Naming Conventions:**
- Tables: use plural, lowercase with underscores (e.g., `orders`, `menu_items`).
- Columns: use snake_case (e.g., `order_id`, `created_at`).
- Foreign keys: `{referenced_table}_id` (e.g., `user_id`).

**API Naming Conventions:**
- Use plural resource names for REST-like endpoints (when present): `/orders`.
- GraphQL field names: use camelCase for field names to match GraphQL conventions.
- Route/parameter formats: use `{id}` for path parameters in docs and OpenAPI examples.

**Code Naming Conventions:**
- C# types and classes: PascalCase (e.g., `OrderItem`).
- C# properties: PascalCase (e.g., `OrderId`).
- JSON exchanged with clients: camelCase (e.g., `orderId`) — server maps between PascalCase and camelCase via serializer settings.

### Structure Patterns

**Project Organization:**
- Projects: `Client` (BlazorWASM), `Server` (ASP.NET Core), `Shared` (DTOs/models).
- Tests: co-locate unit tests with projects using `tests/` solution folder and `*.Tests` projects for server and shared logic.
- Shared utilities: place cross-cutting helpers in `Shared/Utilities` and reference from Client/Server as needed.

**File Structure:**
- Components grouped by feature under `Client/Components/{Feature}`.
- Services under `Client/Services` and server services under `Server/Services`.

### Format Patterns

**API Response Formats:**
- GraphQL responses follow GraphQL spec (data/errors). For REST-like sync endpoints use a minimal wrapper: `{ "data": <payload>, "error": null }` or `{ "error": { "code": "", "message": "" } }`.

**Data Formats:**
- Use ISO 8601 strings for dates in JSON (e.g., `2026-02-26T15:04:05Z`).
- JSON field naming: camelCase for client-facing payloads.

### Communication Patterns

**Event System:**
- Event names: use `entity.action` lowercase with dots (e.g., `order.created`).
- Event payloads: include `version`, `timestamp`, and `correlationId`.

**State Management:**
- Use immutable updates in client store; actions named as `entity/action` (e.g., `order/add`, `order/syncSuccess`).

### Process Patterns

**Error Handling:**
- Standardize error object: `{ code: string, message: string, details?: any }` for REST; GraphQL errors follow GraphQL spec with extensions for `code`.
- All client retryable operations include an `operationId` for idempotency.

**Loading States:**
- Use local component-level loading flags for short operations; global store flags for sync/queue state.

### Enforcement Guidelines

**All AI Agents MUST:**
- Follow these naming and structure patterns exactly in generated code and tests.
- Add or update a `CONTRIBUTING.md` section documenting pattern changes when proposing deviations.
- Include automated linting/format checks in CI that validate naming and JSON formatting rules.

**Pattern Enforcement:**
- Use PR checks (linters, serializers settings) to verify JSON casing and date formats.
- Violations should be reported as PR comments and logged in project issue tracker under `infra/pattern-violations`.

### Pattern Examples

**Good Example:**
- Table: `orders`
- Column: `created_at`
- C# model: `public DateTime CreatedAt { get; set; }`
- JSON payload: `{ "createdAt": "2026-02-26T15:04:05Z" }`

**Anti-Pattern:**
- Mixing snake_case and camelCase in the same API payload or using inconsistent table naming.

---

Appended Implementation Patterns & Consistency Rules. Reply with [A] Advanced Elicitation, [P] Party Mode, or [C] Continue to save and proceed to step-06.

## Project Structure & Boundaries

### Complete Project Directory Structure (concrete)

GoiMon/ (repo root)
- GoiMon.sln
- README.md
- .gitignore
- .editorconfig
- .github/
  - workflows/
    - ci.yml
    - publish.yml
- docs/
  - architecture.md
  - contributing.md
  - api/
- src/
  - Client/                # Blazor WebAssembly (WASM) PWA client
    - GoiMon.Client.csproj
    - Program.cs
    - App.razor
    - wwwroot/
      - index.html
      - manifest.json
      - service-worker.js
    - Pages/
      - Staff/
        - Orders.razor
        - MenuEditor.razor
      - Customer/
        - Menu.razor
    - Components/
      - Shared/
      - Order/
    - Services/
      - ApiService.cs
      - SyncService.cs
    - Stores/
      - LocalStore.cs     # IndexedDB wrapper via EasyAppDev.Blazor.Store
    - Styles/
    - Tests/              # client-focused unit tests (bunit)

  - Server/                # ASP.NET Core server hosting GraphQL + sync endpoints
    - GoiMon.Server.csproj
    - Program.cs
    - appsettings.json
    - GraphQL/
      - Schema.graphql
      - Types/
    - Controllers/        # optional REST-like sync endpoints
      - SyncController.cs
    - Services/
      - OrderService.cs
      - SyncService.cs
    - Data/
      - Migrations/
      - ApplicationDbContext.cs
    - Jobs/
      - ReconciliationJob.cs
    - Tests/              # server unit/integration tests

  - Shared/                # shared DTOs and models between Client and Server
    - GoiMon.Shared.csproj
    - Models/
      - OrderDto.cs
      - MenuItemDto.cs

- infra/
  - docker/
    - Dockerfile.client
    - Dockerfile.server
    - docker-compose.yml
  - raspberry-gateway/     # optional local gateway scripts & docs

- scripts/
  - dev.sh
  - build.sh
  - migrate.sh

- tests/
  - integration/
  - e2e/

### Architectural Boundaries & Mappings

- Staff Order UI → `src/Client/Pages/Staff/Orders.razor`, `src/Client/Components/Order/`
- Menu Management → `src/Client/Pages/Staff/MenuEditor.razor`, `src/Server/Services/OrderService.cs`, `src/Shared/Models/MenuItemDto.cs`
- Offline Store & Sync → `src/Client/Stores/LocalStore.cs`, `src/Client/Services/SyncService.cs`, `src/Server/Controllers/SyncController.cs`, `src/Server/Services/SyncService.cs`
- Customer QR Self-Order → `src/Client/Pages/Customer/Menu.razor`, `src/Server/GraphQL/Types/` (expose menu queries)
- Reconciliation & Exports → `src/Server/Jobs/ReconciliationJob.cs`, `src/Server/Services/OrderService.cs`

### Integration Points & Data Flow (summary)

- Client offline actions are persisted to IndexedDB (`LocalStore`) and queued (`SyncQueue`).
- Sync process uploads batches to `POST /api/sync` (or GraphQL mutation endpoint). Server processes operations idempotently using `operationId` and returns conflict results if needed.
- Server stores canonical state in PostgreSQL (via EF Core) and exposes queries via GraphQL (Hot Chocolate).
- Reconciliation jobs run nightly to compute summaries and export CSVs for owners.

### Project Boundaries

- Client responsibilities: UI, local persistence, presentation logic, offline queue management, retry/backoff for sync.
- Server responsibilities: canonical data store, conflict resolution, auth/authorization, reconciliation, exposing GraphQL schema and sync endpoints.
- Shared responsibilities: DTOs, validation rules, enums, and small shared utilities in `Shared/` project.

---

Appended Project Structure & Boundaries and updated workflow state for step-06. Reply with [A] Advanced Elicitation, [P] Party Mode, or [C] Continue to save and proceed to step-07.

## Architecture Validation Results

### Coherence Validation

- Decision Compatibility: Chosen technologies (Blazor WASM client, ASP.NET Core server, Hot Chocolate GraphQL, PostgreSQL) are compatible. Versions are not pinned in the document — verify `dotnet` SDK and Hot Chocolate versions before scaffold.
- Pattern Consistency: Naming, format, and structure patterns align with the technology choices (C# PascalCase server-side, camelCase client JSON, GraphQL conventions).
- Structure Alignment: Project tree maps cleanly to decisions (Client/Server/Shared projects, sync endpoints, IndexedDB store).

### Requirements Coverage Validation

- Functional Requirements: Staff order UI, menu management, offline queueing/sync, QR customer ordering, reconciliation/export, and onboarding are mapped to concrete files and services.
- Non-Functional Requirements: Offline persistence (IndexedDB + PWA), sync reliability (sync endpoints + `operationId` idempotency), low-end device support (PWA + payload minimization) and security (TLS + device-bound tokens) are addressed.

### Implementation Readiness Validation

- Decision Completeness: Core decisions documented; missing explicit version pins (recommend adding versions for SDKs and key libs).
- Structure Completeness: Directory tree is concrete with key files and component locations specified; test locations included.
- Pattern Completeness: Naming, API formats, event naming, error formats and enforcement guidance are present and actionable.

### Gap Analysis

Critical Gaps:
- Detailed sync protocol (conflict-resolution flows, sample payloads, retry/backoff rules) is not specified and is required before implementing sync logic.
- Explicit idempotency and deduplication rules for `operationId` need concrete examples.

Important Gaps:
- End-to-end test plan for offline→sync→reconcile scenarios.
- Observability: metric/event names, tracing plan, log schema, and backup/restore strategy for the server DB.

Nice-to-Have:
- Payment provider integration architecture (webhook flows, reconciliation hooks).
- Local gateway deployment docs (Raspberry Pi image and sync agent).

### Suggested Immediate Actions

1. Draft a 1–2 page `Sync Protocol` spec with example requests/responses, conflict scenarios, and idempotency rules.
2. Pin runtime/library versions in `docs/architecture.md` and `README.md` (e.g., .NET SDK major, Hot Chocolate major).
3. Add at least one E2E smoke test for offline→sync→reconcile flow and an `e2e` test plan document.
4. Enable CI checks for serializer settings, JSON casing, and lint rules referenced in Enforcement Guidelines.

### Architecture Readiness Summary

Overall Status: READY FOR IMPLEMENTATION (address sync spec gap prior to heavy implementation)

Key Strengths:
- Clear offline-first architecture and concrete file mappings.
- Consistency rules that reduce agent divergence risk.
- Starter template and step-by-step implementation sequence defined.

Areas for Future Enhancement:
- Payment integrations and webhooks design.
- Observability and disaster recovery playbook.

---

Appended validation results. Reply with [A] Advanced Elicitation, [P] Party Mode, or [C] Continue to finalize the architecture and load step-08.

## Completion Summary & Handoff

Congratulations — the Architecture workflow for **GoiMon** is complete.

What we delivered together:
- A validated Architecture Decision Document at `_bmad-output/planning-artifacts/architecture.md` covering project context, starter evaluation, core decisions, implementation patterns, project structure, and validation results.
- Concrete next actions: create a concise `Sync Protocol` spec, pin runtime/library versions, add an E2E smoke test for offline→sync→reconcile, and enable CI checks for serializer/lint rules.

Handoff guidance for implementation:
- First implementation task: scaffold the Blazor WASM hosted solution and add Hot Chocolate to the server (see Starter Initialization commands in the document).
- Implement the `Sync Protocol` before building sync logic; include `operationId` semantics and example payloads.
- Use the `Shared/` project for DTOs and validation rules to keep clients and server consistent.

If you'd like, I can now:
- create the `Sync Protocol` spec draft (1–2 pages), or
- scaffold the initial `GoiMonApp` solution using the recommended `dotnet new` commands, or
- commit these changes and create a PR with CI checks configured.

Reply with which handoff action you'd like next (draft sync spec / scaffold solution / create PR / nothing).


# Sprint 2 Plan — Draft

Owner: Chicuong
Date: 2026-03-07

## Goal
Prepare the codebase and QA artifacts to ship core features (orders, combos, products) and harden CI/contract tests for multi-tenant deployments.

## Backlog (prioritized)

1. S2-01 — Integration & Contract Tests
   - Priority: High
   - Description: Create runnable GraphQL contract tests, wire them to CI, and validate schema/operations. Add smoke scripts to run against local API.
   - Acceptance Criteria: tests run locally and pass; CI job executes tests; failures block merge.
   - Estimate: 3

2. S2-02 — CI/CD Validation & Remote CI Run
   - Priority: High
   - Description: Push changes, run CI workflows, fix any remote-only failures (path filters, secrets, publish steps).
   - Acceptance Criteria: GitHub Actions pass for API and Staff on main branch; deploy steps are no-ops if not configured.
   - Estimate: 2

3. S2-03 — Seed Data / Tenant Consistency Sweep
   - Priority: High
   - Description: Audit seed data, migrations, and any constructors requiring TenantId; update seeds and tests accordingly.
   - Acceptance Criteria: No runtime exceptions from missing tenant context during app startup or tests.
   - Estimate: 2

4. S2-04 — Orders Feature Polish & Bug Fixes
   - Priority: Medium
   - Description: Resolve outstanding order-related validation edge cases, complete missing mutation call-sites, and add unit tests for order flows.
   - Acceptance Criteria: Order unit tests cover create/complete/pay/cancel flows; no regressions.
   - Estimate: 3

5. S2-05 — Cashier Prototype → Blazor Conversion
   - Priority: Medium
   - Description: Convert the S1-09 static prototype into a Blazor page under `GoiMon.Staff/Pages/Checkout` for manual QA iterations.
   - Acceptance Criteria: Clickable cashier UI running in local WASM with sample flows.
   - Estimate: 5

6. S2-06 — StrawberryShake Regeneration & Client Sanity
   - Priority: Medium
   - Description: Ensure all `.graphql` files are correct and StrawberryShake client is generated as part of the build; add a validation step.
   - Acceptance Criteria: `dotnet build` regenerates clients; client code compiles without warnings relevant to GraphQL types.
   - Estimate: 1

7. S2-07 — E2E Smoke Tests
   - Priority: Low
   - Description: Add a basic Playwright/Puppeteer smoke test suite hitting the main flows (create order, pay, ticket printing stub).
   - Acceptance Criteria: Smoke tests run locally and in CI stage job.
   - Estimate: 4

8. S2-08 — Docs & BMAD Artifacts
   - Priority: Low
   - Description: Update `_bmad-output` planning artifacts with Sprint 2 stories and assign owners; collect ADKs/ADRs if changed.
   - Acceptance Criteria: Sprint 2 plan exists and is reviewable; ticket board seeded.
   - Estimate: 1

## Next Steps
- Confirm priorities and owners.
- I'll push this artifact to the repo when you confirm.

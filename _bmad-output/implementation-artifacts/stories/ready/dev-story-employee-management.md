# 👥 Dev Story: Employee Management (Owner/Staff Accounts)

**Status:** Ready  
**Date Created:** 2026-03-05  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 4-1-employee-management

---

## Story

As an **owner**,  
I want to **create and manage staff accounts with simple roles (owner/staff)**,  
so that **my restaurant team can operate with least-privilege access and clear accountability**.

---

## Scope

### In Scope
- Employee account list with status (`Active` / `Inactive`)
- Create employee account (name, phone/email, role)
- Update employee profile and role
- Activate/deactivate employee accounts
- Basic role model: `Owner`, `Staff`
- Role-based authorization guard for employee-management actions
- Audit metadata fields for employee records (`CreatedAt`, `UpdatedAt`, optional `CreatedBy`)

### Out of Scope
- Payroll, attendance, shift scheduling
- Fine-grained RBAC/permission matrix beyond owner/staff
- Multi-tenant cross-store staffing
- Password reset flows beyond current authentication baseline

---

## Dependencies

- Existing authentication baseline and user aggregate from:
  - `src/GoiMon.Api/Domain/Entities/User.cs`
  - `src/GoiMon.Api/Features/Authentication/*`
- Existing GraphQL + StrawberryShake patterns used in current CRUD stories
- State management baseline in client pages (`StoreComponentWithUtilities<TState>`)
- PRD requirement alignment:
  - `FR25`: Owner can manage staff accounts and assign owner/staff roles
  - `FR26`: authenticated sessions + role-based protection

---

## Acceptance Criteria

- [ ] **AC1**: API provides employee query with paging/filter/sort for owner role only.
- [ ] **AC2**: API provides employee create mutation with required fields and role assignment (`Owner`/`Staff`).
- [ ] **AC3**: API provides employee update mutation (profile + role) for owner role only.
- [ ] **AC4**: API provides activate/deactivate mutation; inactive staff cannot execute staff-only protected actions.
- [ ] **AC5**: Client has Employee Management screen with list/search/sort and create/edit/status toggle actions.
- [ ] **AC6**: Role options in UI are constrained to `Owner` and `Staff` with clear labels.
- [ ] **AC7**: Authorization guards enforce only `Owner` can access employee-management page and mutations.
- [ ] **AC8**: Realtime/state refresh behaves consistently after create/update/deactivate operations.
- [ ] **AC9**: API + Client build passes and core employee-management flow is manually verifiable.

---

## Task Breakdown

### TASK 1 — Domain + Data Model (AC: #2, #3, #4, #6)
- [ ] Add/confirm role field on user model (`Owner`/`Staff`) and active flag semantics.
- [ ] Add role enum/value object and validation constraints.
- [ ] Ensure migration updates schema safely for existing users.

**Files:**
- `src/GoiMon.Api/Domain/Entities/User.cs`
- `src/GoiMon.Api/Domain/Enums/*` (new if needed)
- `src/GoiMon.Api/Infrastructure/Persistence/Migrations/*`

### TASK 2 — API GraphQL: Employee Feature (AC: #1, #2, #3, #4, #7)
- [ ] Create feature folder and GraphQL queries/mutations for employees.
- [ ] Add owner-only authorization on all employee-management operations.
- [ ] Add validators for create/update payloads.

**Files:**
- `src/GoiMon.Api/Features/Employees/Queries/EmployeeQueries.cs`
- `src/GoiMon.Api/Features/Employees/Mutations/EmployeeMutations.cs`
- `src/GoiMon.Api/Features/Employees/Validators/*`
- `src/GoiMon.Api/Features/Employees/Models/*`

### TASK 3 — Client GraphQL + Feature State (AC: #1, #2, #3, #4, #8)
- [ ] Add GraphQL operations for employee list/create/update/toggle status.
- [ ] Add `EmployeesUiState` store and cache structure.
- [ ] Wire generated StrawberryShake operations.

**Files:**
- `src/GoiMon.Client/GraphQL/Employees/*.graphql`
- `src/GoiMon.Client/State/EmployeesUiState.cs`
- `src/GoiMon.Client/Program.cs` (store registration)

### TASK 4 — Client UI: Employee Management Page (AC: #5, #6, #7, #8)
- [ ] Add page `Pages/Employees.razor` (owner-only route/guard).
- [ ] Add employee table with search/sort/status badges and actions.
- [ ] Use shared dialog components for create/edit/confirm status change where possible.

**Files:**
- `src/GoiMon.Client/Pages/Employees.razor`
- `src/GoiMon.Client/Features/Employees/Components/*`
- `src/GoiMon.Client/Shared/Components/ConfirmDialog.razor` (reuse only)
- `src/GoiMon.Client/Shared/MainLayout.razor` (sidebar nav link)

### TASK 5 — Verification + Test Coverage (AC: #9)
- [ ] Add API tests for role guard and employee CRUD/toggle flow.
- [ ] Add client-level smoke validation checklist for owner/staff behavior.
- [ ] Validate unauthorized access denied for staff users.

**Files:**
- `tests/GoiMon.Api.Tests/Features/Employees/*`
- `tests/GoiMon.Client.Tests/Features/Employees/*` (if existing test style applies)
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-employee-management.md` (status/log updates)

---

## Verification Plan

- [ ] Build command(s):
  - `dotnet build src/GoiMon.Api/GoiMon.Api.csproj`
  - `dotnet build src/GoiMon.Client/GoiMon.Client.csproj`
- [ ] Manual scenario(s):
  - Owner creates staff, edits role, deactivates staff, confirms staff access blocked.
- [ ] Edge case(s):
  - Prevent self-demotion for last remaining owner.
  - Prevent deactivating last active owner.
  - Handle duplicate email/phone conflict gracefully.

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
- [ ] Manual validation completed
- [ ] Status and folder updated to matching state
- [ ] Story board row updated

---

## Dev Notes

### Design Decisions
1. Start with simple role model (`Owner`, `Staff`) to match PRD FR25 and reduce implementation risk.
2. Keep employee management owner-only and align with existing auth/session architecture.
3. Reuse existing shared dialogs and state patterns to keep UI behavior consistent.

### Risks
- Existing auth model may require migration strategy to backfill roles for current users.
- Authorization gaps if mutations are protected but client route guard is missing.

### Resolved Product Decisions (for implementation)
- Account identity in MVP: **email is mandatory**, phone is optional.
- Owner creation policy: **owner can create another owner**.
- Onboarding mode: **direct account creation only** in MVP (invite-based onboarding is post-MVP).

---

## Change Log

- 2026-03-05 — Story created in `backlog/` by Mary (Business Analyst)
- 2026-03-05 — Story refined and moved `Backlog -> Ready` by Mary (Business Analyst)

# 📸 Dev Story: UGC Campaign Lite (Photo Challenge + Share Card)

**Status:** Ready  
**Date Created:** 2026-03-05  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 5-2-ugc-campaign-lite

---

## Story

As a **merchant owner**,  
I want to **run a simple weekly customer photo challenge with shareable reward cards**,  
so that **customers create social proof and organically bring new visitors**.

---

## Scope

### In Scope
- Merchant creates a lightweight campaign (`title`, `start/end`, `reward`, `rules summary`)
- Customer submits one photo entry per order reference (link-based upload flow)
- Staff/owner approves or rejects submissions
- Public campaign gallery page (approved entries only)
- Share card generation for approved winners (download/share link)

### Out of Scope
- Native social API publishing (TikTok/Facebook Graph write APIs)
- AI moderation pipeline
- Multi-campaign scheduling with advanced segmentation
- Multi-merchant leaderboard

---

## Dependencies

- `5-1-viral-growth-foundation` (growth settings and baseline tracking)
- Existing image upload capability (`4-1-image-upload`)
- Owner/staff role guards for moderation actions

---

## Acceptance Criteria

- [ ] **AC1**: Owner can create/update/deactivate a UGC campaign with required fields and valid date range.
- [ ] **AC2**: Customer can submit one photo entry tied to a valid order reference during campaign window.
- [ ] **AC3**: Staff/owner can approve/reject entries; only approved entries appear in gallery.
- [ ] **AC4**: System generates a winner share card with campaign/reward metadata and shareable link.
- [ ] **AC5**: Basic campaign metrics are visible: submissions, approvals, shares.
- [ ] **AC6**: API + Client builds pass and end-to-end submission/moderation flow is manually verified.

---

## Task Breakdown

### TASK 1 — API/Domain (AC: #1, #2, #3, #4)
- [ ] Add campaign and submission entities with moderation state.
- [ ] Add GraphQL operations for campaign CRUD, submit entry, approve/reject, and metrics query.
- [ ] Enforce one-entry-per-order-per-campaign rule.

**Files:**
- `src/GoiMon.Api/Features/Growth/*`
- `src/GoiMon.Api/Domain/Entities/*` (campaign/submission entities)

### TASK 2 — Client/UI (AC: #1, #2, #3, #4, #5)
- [ ] Add owner campaign management UI.
- [ ] Add public campaign gallery and submission page.
- [ ] Add moderation panel and winner share-card action.

**Files:**
- `src/GoiMon.Client/Pages/Growth.razor`
- `src/GoiMon.Client/Pages/Campaign.razor`
- `src/GoiMon.Client/Features/Growth/Components/*`

### TASK 3 — Validation/Tests (AC: #6)
- [ ] Add tests for date-window validation and one-entry-per-order rule.
- [ ] Add manual test checklist for moderation and public gallery visibility.

**Files:**
- `tests/GoiMon.Api.Tests/Features/Growth/*`
- `tests/GoiMon.Client.Tests/Features/Growth/*` (if current test style applies)

---

## Verification Plan

- [ ] Build command(s):
  - `dotnet build src/GoiMon.Api/GoiMon.Api.csproj`
  - `dotnet build src/GoiMon.Staff/GoiMon.Staff.csproj`
- [ ] Manual scenario(s):
  - Owner creates campaign, customer submits photo, staff approves, entry appears in gallery, winner share card generated.
- [ ] Edge case(s):
  - Reject submissions outside campaign window.
  - Block duplicate submission for same order in same campaign.

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
1. Keep campaign model intentionally small to maximize speed-to-market.
2. Reuse existing growth and image-upload infrastructure.
3. Require moderation before public display to limit inappropriate content risk.

### Risks
- Manual moderation load can grow quickly if submissions spike.
- Abuse/spam risk without stricter identity checks.

### Resolved Product Decisions
- Winner selection in MVP: **manual only** by owner/staff (no random picker in MVP).
- Gallery visibility in MVP: **public short-link page** for maximum sharing reach.

---

## Change Log

- 2026-03-05 — Story created in `backlog/` by Mary (Business Analyst)
- 2026-03-05 — Story refined (open questions resolved) and moved `Backlog -> Ready` by Mary (Business Analyst)

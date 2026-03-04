# GoiMon Story Management Board

**Last Updated:** 2026-03-05  
**Owner:** Chicuong  
**Purpose:** Single place to track story status, priority, dependencies, and execution order.

---

## 1) Status Standard (Use this consistently)

- `Backlog` — not started
- `Ready` — fully defined, ready to implement
- `In Progress` — currently being developed
- `Blocked` — cannot move due to dependency
- `Review` — implementation done, waiting QA/review
- `Done` — accepted and completed

---

## 2) Master Story Register

| Story Key | Story | Area | Current Source Status | Normalized Status | Priority | Dependency | Next Action |
|---|---|---|---|---|---|---|---|
| 3-2-order-payment | Mark Order as Paid (Cashier Flow) | API+Client/Orders | ready-for-dev | Ready | P0 | Order flow baseline done | Implement mutation + UI action + badge + refresh path |
| 3-3-table-management-core | Table Management Core (Virtual Slots + Service/Payment Tracking) | API+Client/Tables+Orders | ready-for-dev | Ready | P0 | 3-0-order-lifecycle-core + 3-2-order-payment alignment | Start implementation: TableSlot domain + GraphQL tables feature + table board UI |
| 5-1-viral-growth-foundation | Viral Growth Foundation (Referral + Share + Streak) | API+Client/Growth | ready-for-dev | Ready | P1 | 3-2-order-payment + 3-3-table-management-core | Start implementation: growth domain + referral/share/streak UI and metrics |
| 5-2-ugc-campaign-lite | UGC Campaign Lite (Photo Challenge + Share Card) | API+Client/Growth | ready-for-dev | Ready | P2 | 5-1-viral-growth-foundation + 4-1-image-upload | Start implementation: campaign CRUD + moderation + public gallery/share-card |
| 2-1-auth-ui-blazor | Authentication UI Implementation | Client/Auth | ready-for-dev (+partial tasks done) | In Progress | P0 | Auth API completed | Replace stubs with StrawberryShake operations, finish protected routes/logout/tests |
| 4-1-employee-management | Employee Management (Owner/Staff Accounts) | API+Client/Employees | ready-for-dev | Ready | P0 | Auth baseline + role model alignment | Start implementation: domain role model + employee GraphQL feature |
| 1-1-category-management | Category Management (CRUD + Query) | API+Client/Categories | implemented | Done | P1 | None | Keep in monitoring only |
| 1-2-product-management | Product Management (CRUD + Variants + Modifiers) | API+Client/Products | implemented | Done | P1 | Category feature baseline | Keep in monitoring only |
| 1-3-combo-management | Combo Management (CRUD + Combo Items) | API+Client/Combos | implemented | Done | P1 | Product feature baseline | Keep in monitoring only |
| 3-0-order-lifecycle-core | Order Lifecycle Core (Create + Complete + Cancel + Subscription) | API+Client/Orders | implemented | Done | P0 | Product + combo baseline | Keep in monitoring only |
| 4-1-image-upload | Image Upload Service Integration | API+Client/Platform | implemented | Done | P1 | Cloudinary config | Keep in monitoring only |
| 3-1-order-combo | Order Combo Support | API+Client/Orders | implemented | Done | P0 | None | Keep in monitoring only |
| auth-api-foundation | User Authentication with OAuth + OTP | API/Auth | COMPLETED | Done | P0 | None | Keep in monitoring only |

---

## 3) Suggested Execution Queue (for easiest management)

1. **Now:** `3-2-order-payment` (small, isolated, high cashier value)
2. **Then:** `3-3-table-management-core` (extends service/payment visibility for dine-in operations)
3. **Growth Sprint Block A:** `5-1-viral-growth-foundation` (referral + share + streak for low-cost acquisition)
4. **Growth Sprint Block B:** `5-2-ugc-campaign-lite` (community photos + share card)
5. **Then:** Finish remaining of `2-1-auth-ui-blazor` (remove stubs + complete AC7/AC8/AC12)
6. **Parallel monitor:** `3-1-order-combo`, `auth-api-foundation` as done stories

---

## 4) Definition of Ready (DoR) Checklist

Before moving any story to `Ready`:

- [ ] Story has unique `Story Key`
- [ ] Scope explicitly says in-scope/out-of-scope
- [ ] Acceptance Criteria are testable
- [ ] Dependencies listed
- [ ] API + Client impact identified
- [ ] Build/test verification plan included

---

## 5) Definition of Done (DoD) Checklist

Before moving any story to `Done`:

- [ ] All acceptance criteria checked
- [ ] `dotnet build` passes for impacted projects
- [ ] Manual verification notes captured
- [ ] Story status updated in source file
- [ ] This board updated (status + next action removed)

---

## 6) Weekly Ritual (15 minutes)

- Reclassify each story into one normalized status
- Keep max **2 stories** in `In Progress` (WIP limit)
- Promote at most **1 P0 story** into active work at a time
- Archive done stories after 2 weeks to a `Done History` section

---

## 7) Source Story Files

- `_bmad-output/implementation-artifacts/stories/in-progress/dev-story-authentication-ui.md`
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-employee-management.md`
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-table-management-core.md`
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-viral-growth-foundation.md`
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-ugc-campaign-lite.md`
- `_bmad-output/implementation-artifacts/stories/ready/dev-story-order-payment.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-order-combo.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-authentication.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-category-management.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-product-management.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-combo-management.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-order-lifecycle-core.md`
- `_bmad-output/implementation-artifacts/stories/done/dev-story-image-upload.md`

---

## 8) Active Story Folder Structure

Current structure under `_bmad-output/implementation-artifacts/stories/`:

- `backlog/`
- `ready/`
- `in-progress/`
- `blocked/`
- `review/`
- `done/`

Rule: when status changes, move the story file to the matching folder and update row in section 2.

---

## 9) Story Template (Use for all new stories)

- Template file: `_bmad-output/implementation-artifacts/stories/_templates/dev-story-template.md`
- Folder guide: `_bmad-output/implementation-artifacts/stories/README.md`

Quick flow:

1. Copy template into `stories/backlog/`
2. Fill placeholders (`Story Key`, `AC`, `Task Breakdown`, `Verification Plan`)
3. Add row in section 2 (Master Story Register)
4. Move file folder whenever status changes (`Backlog -> Ready -> In Progress -> Review -> Done`)

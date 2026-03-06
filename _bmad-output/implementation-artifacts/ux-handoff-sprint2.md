# UX Spec Handoff Package — Sprint 2 Build

**Story:** S1-10  
**Date:** 2026-03-07  
**Source Prototype:** S1-09 (`/prototype/cashier`)  
**Policy Matrix:** S1-03 (`role-permission-matrix.md`)

---

## 1. Design Tokens & Spacing Scale

### Color Tokens (Semantic — from theme.css)

Use CSS custom properties exclusively. Never hardcode hex/oklch in Razor files.

- `bg-background` — page/app background
- `bg-card` — card surfaces, sidebars
- `bg-muted` — secondary surfaces, placeholders
- `bg-primary` — primary action fills
- `bg-destructive` — error/danger fills
- `text-foreground` — primary text
- `text-muted-foreground` — secondary/hint text
- `text-primary` — accent text, prices, active labels
- `text-primary-foreground` — text on primary background
- `text-destructive` — error text
- `border` — default borders
- `border-primary` — active/selected borders

### Spacing Scale (Tailwind)

Consistent spacing steps across all screens:

- **Micro gap:** `gap-2` (8px) — inline icon+label, tight groups
- **Standard gap:** `gap-3` / `gap-4` (12–16px) — card content, form fields
- **Section padding:** `p-4` / `p-6` (16–24px) — card/panel padding
- **Page padding:** `p-6` (24px) — outer page container
- **Large separation:** `mb-6` / `mb-8` (24–32px) — section separators

### Layout Grid

- **Table Grid:** `grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4`
- **Product Grid:** `grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4`
- **Cart Sidebar:** fixed `w-96` right panel with `border-l`
- **Responsive:** Grid collapses to 2 columns on mobile. Cart sidebar becomes full-width overlay below `md`.

### Typography

- **Page title:** `text-3xl font-bold tracking-tight` (Table Grid header)
- **Section title:** `text-2xl font-bold` (Ordering View header)
- **Card title:** `text-sm font-bold` (product name, table name)
- **Price:** `text-sm font-bold text-primary` (product card), `text-xl font-black text-primary` (cart total)
- **Label/meta:** `text-[10px] font-bold uppercase tracking-widest text-muted-foreground`
- **Badge count:** `text-[10px] font-bold` in `rounded-full bg-primary/10 text-primary`

---

## 2. Component Inventory (BlazorBlueprint)

### Buttons

- **Primary action** (Checkout, Continue): `<BbButton Size="ButtonSize.Small">` with custom `rounded-xl py-6` for large touch target
- **Secondary action** (Cancel Order): `<BbButton Variant="ButtonVariant.Outline" Size="ButtonSize.Small">` with `rounded-xl py-6`
- **Icon-only action** (Qty +/−): native `<button>` with `w-6 h-6` min target, `rounded-md`
- **Filter/Action bar**: `<BbButton Variant="ButtonVariant.Outline" Size="ButtonSize.Small">` with `<LucideIcon>` prefix

### Switches

- **Connection toggle**: `<BbSwitch Checked="..." CheckedChanged="...">` inside `ConnectionStatus.razor`

### Icons

- All icons via `<LucideIcon Name="..." Class="h-N w-N" />`
- Standard sizes: `h-3 w-3` (inline), `h-4 w-4` (button icon), `h-5 w-5` (nav), `h-8 w-8` (modal feature), `h-10 w-10` (hero), `h-16 w-16` (empty state)

### Avatar

- `<BbAvatar Size="AvatarSize.Small">` with `<BbAvatarFallback>` for user display in header

### Toast/Dialog/Portal

- `<BbToastProvider Position="ToastPosition.BottomRight" />` — notification position
- `<BbDialogProvider />` — modal/dialog hosting
- `<BbPortalHost />` — portal rendering root

### Cards (Product, Table)

- Not using `<BbCard>` — custom `<button>` / `<a>` elements with `rounded-2xl border bg-card/40 backdrop-blur-sm` for interactive cards with hover effects (`hover:shadow-xl hover:-translate-y-1`)

### Sidebar

- `<BbSidebarProvider>` wraps the layout for sidebar context

---

## 3. Screen State Catalog

### 3.1 Table Grid View (`/prototype/cashier`)

| State | Visual Behavior |
|-------|----------------|
| **Default** | Grid of table cards. Each card shows name, capacity, status indicator (color dot + ribbon for "Serving"). |
| **Empty (no tables)** | Center: `<LucideIcon Name="table-2" Class="h-16 w-16" />` + "Chưa có bàn nào" label. `text-muted-foreground/30`. |
| **Loading** | `<BbSpinner />` centered in grid area. Cards replaced with skeleton placeholders (`bg-muted animate-pulse rounded-2xl`). |
| **Error (fetch failed)** | `<BbAlert Variant="AlertVariant.Danger">` with retry button. Message: "Không tải được danh sách bàn." |
| **Offline** | Sticky top banner: `bg-destructive/20` with `<LucideIcon Name="wifi-off" />` + "Mất kết nối — Đơn hàng sẽ được lưu tạm tại thiết bị". Tables still visible from local cache. |

### 3.2 Ordering View (`/prototype/cashier/order/{TableId}`)

| State | Visual Behavior |
|-------|----------------|
| **Default** | Left: product grid with category tabs + search. Right: cart sidebar. |
| **Empty cart** | Cart sidebar center: `<LucideIcon Name="shopping-basket" Class="h-16 w-16" />` + "Chưa có món nào". Checkout button disabled. |
| **Loading products** | Product grid: skeleton cards (`bg-muted animate-pulse`). Category tabs disabled. |
| **Error (products)** | `<BbAlert>` in product grid area with retry. Cart still functional if cached data. |
| **Search no results** | Product grid empty + "Không tìm thấy món phù hợp" centered text. |
| **Offline** | Top banner (same as Table Grid). Products served from local cache. Cart functional. Payment modal shows "Lưu tạm" instead of process. |

### 3.3 Payment Modal

| State | Visual Behavior |
|-------|----------------|
| **Default** | Centered overlay with `bg-black/40 backdrop-blur-md`. Two payment options: Cash, Bank Transfer. |
| **Processing** | Both buttons disabled. `<BbSpinner />` replaces icon on selected method. |
| **Success** | Full-screen `bg-primary` overlay with check icon, "THÀNH CÔNG!" text, auto-redirect after 2s or "TIẾP TỤC" button. |
| **Error** | `<BbAlert Variant="AlertVariant.Danger">` inside modal: "Thanh toán thất bại. Vui lòng thử lại." with retry button. |
| **Offline** | Cash option only. Bank Transfer disabled with tooltip: "Cần kết nối mạng". Cash records to local queue. |

### 3.4 Connection Status Component

| State | Visual Behavior |
|-------|----------------|
| **Online** | Green dot (`bg-primary`), label "Trực tuyến", switch ON. `bg-primary/10 border-primary/20`. |
| **Offline** | Red pulsing dot (`bg-destructive animate-pulse`), label "Chế độ Ngoại tuyến", switch OFF. `bg-destructive/10 border-destructive/20`. |

---

## 4. Interaction & Accessibility Notes

### Touch Targets

- **Minimum touch target:** 44×44px (per WCAG 2.5.8)
- **Primary action buttons** (Checkout, Continue): `py-6` on `BbButton` = ~56px height
- **Product cards:** Full card is tap target (`aspect-square` + padding ≥ 48px)
- **Table cards:** Full card is tap target via `<a>` wrapper
- **Qty +/− buttons:** `w-6 h-6` (24px) — below minimum. **Sprint 2 action: increase to `w-8 h-8` (32px) min, ideally `w-10 h-10` (40px)**
- **Category tabs:** `px-4 py-2` = adequate at ~40px height

### Keyboard Shortcuts (Sprint 2 Implementation Targets)

- `Escape` — Close payment modal, close any open dialog
- `Enter` — Confirm payment when modal is open
- `/` or `Ctrl+K` — Focus product search input
- `Tab` — Navigate: category tabs → product grid → cart items → action buttons
- `Arrow keys` — Navigate within product grid (optional enhancement)

### Focus Management

- **Payment modal open:** Trap focus within modal. First focus: Cash button. `Escape` closes.
- **Success screen:** Focus on "TIẾP TỤC" button.
- **Search input:** Auto-focus not applied on page load (preserve table/product exploration). Manual focus via `/` shortcut.
- **Category tabs:** Left/Right arrow key navigation between tabs.

### Animations & Transitions

- **Card hover:** `hover:-translate-y-1 transition-all duration-300`
- **Modal enter:** `animate-in fade-in duration-300` (overlay), `animate-in zoom-in-95 duration-300` (content)
- **Cart item add:** `animate-in slide-in-from-right duration-300`
- **Offline banner:** `animate-in slide-in-from-top duration-300`
- **Success screen:** `animate-in fade-in duration-500` (overlay), `animate-in zoom-in-50 duration-500` (content)

---

## 5. Role-Based UI Behavior

Reference: `role-permission-matrix.md` (S1-03)

### Table Grid View

| Element | Cashier | Supervisor | Manager | Owner |
|---------|---------|------------|---------|-------|
| View tables | ✅ | ✅ | ✅ | ✅ |
| "Mở bàn nhanh" button | ✅ | ✅ | ✅ | ✅ |
| "Lọc khu vực" filter | ✅ | ✅ | ✅ | ✅ |

### Ordering View

| Element | Cashier | Supervisor | Manager | Owner | Policy |
|---------|---------|------------|---------|-------|--------|
| Add items to order | ✅ | ✅ | ✅ | ✅ | — |
| Edit open order | ✅ | ✅ | ✅ | ✅ | `Policies.Order.EditPrePayment` |
| Void/cancel order | 🔑 PIN | ✅ | ✅ | ✅ | `Policies.Order.Void` |
| Delete order (wipe) | ❌ Hidden | ❌ Hidden | ✅ | ✅ | `Policies.Order.HardDelete` |

Legend: ✅ Allow | 🔑 PIN = Supervisor Override PIN required | ❌ Hidden = element not rendered

### Cart / Checkout

| Element | Cashier | Supervisor | Manager | Owner | Policy |
|---------|---------|------------|---------|-------|--------|
| "Hủy đơn" (cancel) | 🔑 PIN | ✅ | ✅ | ✅ | `Policies.Order.Void` |
| "Thanh toán" (pay) | ✅ | ✅ | ✅ | ✅ | — |
| Edit post-payment | 🔑 PIN | ✅ | ✅ | ✅ | `Policies.Order.EditPostPayment` |
| Reprint receipt | ✅ | ✅ | ✅ | ✅ | `Policies.Order.Reprint` |

### Supervisor Override PIN Flow

1. Cashier taps restricted action (e.g. "Hủy đơn").
2. PIN dialog opens: `<BbDialog>` with `<BbInputOTP>` (6-digit numeric).
3. Header: "Yêu cầu phê duyệt" + action description.
4. Supervisor enters PIN → API validates → action proceeds or shows error.
5. PIN dialog auto-closes on success. Shows `<BbAlert Variant="AlertVariant.Danger">` on failure with retry.

### Navigation Visibility

| Nav Element | Cashier | Supervisor | Manager | Owner | Accountant | Policy |
|-------------|---------|------------|---------|-------|------------|--------|
| Reports tab | ❌ | ✅ (shift) | ✅ (store) | ✅ (all) | ✅ | `Policies.Reports.View` |
| Inventory Adjustment | ❌ | 🔑 PIN | ✅ | ✅ | ❌ | `Policies.Inventory.Adjust` |

---

## 6. Sprint 2 Implementation Notes

### Known Gaps from Prototype

1. **Qty buttons too small** — Current `w-6 h-6` (24px). Increase to `w-10 h-10` (40px) for touch compliance.
2. **No loading states** — Prototype uses mock data. Add `<BbSpinner>` and skeleton placeholders for all data fetches.
3. **No error states** — Add `<BbAlert>` with retry for failed GraphQL queries.
4. **Cart sidebar not responsive** — Below `md` breakpoint, convert to bottom sheet or full-screen overlay.
5. **Product descriptions are placeholder** — Replace `"Lorem ipsum..."` with actual `product.Description` from API.
6. **No keyboard shortcuts** — Implement `Escape`, `Enter`, `/` shortcuts in Sprint 2.
7. **No supervisor PIN modal** — Build `<SupervisorPinDialog>` shared component per Section 5 spec.

### Component Build Priority (Sprint 2)

1. `SupervisorPinDialog.razor` — Shared component for approval-required actions
2. `LoadingOverlay.razor` / skeleton states — Applied to all data-fetch screens
3. `ErrorAlert.razor` — Standardized retry-able error for GraphQL failures
4. `OfflineQueueIndicator.razor` — Shows pending sync count when offline
5. Responsive cart — Bottom sheet pattern for mobile

### Data Integration Targets

- **Table Grid:** Replace `TableMock` with `GetTableSlots` GraphQL query (existing `TableQueries`)
- **Product Grid:** Replace `ProductMock` with `GetProducts` query + category filter (existing `ProductQueries`)
- **Cart:** Use `OrderMutations.CreateOrder` with point-in-time snapshots
- **Payment:** Placeholder — no real payment in Sprint 2. Mark as paid via `OrderMutations`

---

## 7. Handoff Checklist

- [x] Component inventory with BlazorBlueprint names, variants, sizes
- [x] Spacing scale and layout grid defined
- [x] Typography scale documented
- [x] Color tokens mapped to semantic CSS variables
- [x] Empty state defined per screen
- [x] Loading state defined per screen
- [x] Error state defined per screen
- [x] Success state defined per screen
- [x] Offline/degraded state defined per screen
- [x] Touch target minimums documented
- [x] Keyboard shortcuts specified
- [x] Focus management rules defined
- [x] Animation/transition specs listed
- [x] Role-based visibility mapped to policy names
- [x] Supervisor Override PIN flow specified
- [x] Known gaps and Sprint 2 action items listed
- [x] Data integration targets mapped to existing API

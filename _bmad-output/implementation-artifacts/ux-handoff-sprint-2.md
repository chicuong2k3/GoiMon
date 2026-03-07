# 🎨 UX Handoff Package: Sprint 2 (POS Implementation)

**Status:** Ready for Engineering  
**Version:** 1.0  
**Date:** 2026-03-07  

## 1. Design System Tokens

### Color Palette (Glassmorphism Optimized)
| Token | Hex/HSL | Usage |
| :--- | :--- | :--- |
| **Primary** | `hsl(142, 71%, 45%)` | Brand, Success, Primary Actions |
| **Secondary** | `hsl(215, 25%, 27%)` | Subtle Actions, Borders |
| **Background** | `hsl(210, 40%, 96%)` | Main App Surface |
| **Card/Glass** | `rgba(255, 255, 255, 0.4)` | Glassmorphic containers (backdrop-blur: 12px) |
| **Destructive** | `hsl(0, 84%, 60%)` | Errors, Offline Warnings, Deletions |

### Typography
- **Primary Font**: `Outfit` or `Inter` (Sans-serif)
- **Headings**: Semibold/Black (tracking-tighter)
- **Body**: Regular (14px baseline)
- **Interactive**: Bold, uppercase, tracking-widest (10px - 12px)

---

## 2. Core Components Library

### A. Buttons (`BbButton`)
- **Primary**: Solid green, high shadow. Used for "Thanh toán", "Xác nhận".
- **Outline**: 1px border, transparent bg. Used for "Hủy", "Quay lại".
- **Ghost**: No border, hover bg change. Used for icon buttons in lists.
- **Icon-only**: 40x40px or 32x32px.

### B. Cards (`BbCard`)
- **Standard**: rounded-2xl, border-border, bg-card/40.
- **Glass**: backdrop-blur-xl, border-white/20, bg-white/40.

### C. Status Indicators
- **Connection Status**: Pulse dot (Green = Online, Red = Offline).
- **Table Badge**: 
  - `Available`: Green/Outline
  - `Occupied`: Solid Green
  - `Dirty/Wait`: Amber/Outline

---

## 3. Interaction States & Transitions

### A. Feedback States
- **Success**: Full-screen green overlay with scale-in checkmark (as seen in `OrderingView.razor`).
- **Loading**: Skeleton screens for table grids and category lists.
- **Empty**: Centered icon + uppercase helper text (e.g., "Chưa có món nào").

### B. Offline Experience (AC3)
- **Visibility**: Sticky top-bar or status-bar indicator.
- **Constraint**: Disable "Cloud Sync" specific actions, but KEEP order-entry enabled.
- **Visual**: Apply `grayscale` or `opacity-50` to non-local features.

---

## 4. Key Flows (Handoff to Dev)

| Flow | Prototype Reference | Key Component |
| :--- | :--- | :--- |
| **Quick Order** | `/prototype/cashier/order/{id}` | `ProductGrid`, `CartPanel` |
| **Table Switch** | `/prototype/cashier` | `TableGridView` |
| **Payment Success**| `showSuccess = true` | `SuccessModal` (Animated) |

## 5. Engineering Sign-off
- [x] All components use `BlazorBlueprint` base where possible.
- [x] Tailwind CSS classes verified for JIT compilation.
- [x] Responsive breakpoints validated for 360px (Mobile) and 1024px (Tablet).

---
*Created by Antigravity AI for GoiMon Project.*

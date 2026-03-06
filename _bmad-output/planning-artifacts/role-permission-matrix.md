# Role-Permission Matrix for Critical POS Actions

**Related Story:** S1-03
**Status:** Defined (Awaiting Signoff)
**Owners:** PM/BA + Tech Lead

## 1. System Roles Definition

| Role | Scope & Responsibilities |
| --- | --- |
| **Cashier** | Standard operator at the POS terminal handling daily orders, simple modifications, and payments. |
| **Supervisor** | Shift leader capable of handling minor escalations, overriding voids, and managing shift activities. |
| **Manager** | Store/branch manager responsible for venue operations, high-level overrides, adjustments, and store reports. |
| **Owner** | Full administrative access across the system (can manage multiple stores, settings, billing). |
| **Accountant** | Back-office user focusing on financial reports, invoice auditing, and settlements. No POS operational power. |

## 2. Core Actions matrix

The matrix defines whether an action is:
- **Allow**: User can perform the action independently.
- **Deny**: User cannot perform the action.
- **Approval-Required**: User initiates the action, but a higher-privileged user (Supervisor/Manager) must enter their PIN/Credentials to authorize it in real-time.

| Action Category | Specific Action | Cashier | Supervisor | Manager | Owner | Accountant |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Order Ops** | Edit Order (Pre-Payment) | Allow | Allow | Allow | Allow | Deny |
| **Order Ops** | Edit Order (Post-Payment) | Approval-Required | Allow | Allow | Allow | Deny |
| **Order Ops** | Void Order (Cancel Unpaid) | Approval-Required | Allow | Allow | Allow | Deny |
| **Order Ops** | Delete Order (Wipe Record) | Deny | Deny | Allow | Allow | Deny |
| **Print Ops** | Reprint Invoice / Receipt | Allow | Allow | Allow | Allow | Allow |
| **Finance/Ops** | Shift Close (Till Count) | Allow* | Allow | Allow | Allow | Deny |
| **Inventory** | Stock Adjustment | Deny | Approval-Required | Allow | Allow | Deny |
| **Reporting** | Report Access | Deny | Allow (Shift only) | Allow (Store) | Allow (All) | Allow |

*(Note: Cashiers can "Submit" their shift close, but Supervisor/Manager typically verifies the amount. For this matrix, submitting their own shift is an "Allow" action).*

## 3. UI Behavior & API Policy Mapping

This section satisfies the requirement to map the abstract rules into concrete developer instructions for the UI guards and API authorization boundaries.

| Specific Action | UI Behavior (When Denied/Unauthorized) | API Policy Name |
| :--- | :--- | :--- |
| **Edit Order (Pre-Payment)** | Disable "Edit" button if not allowed. | `Policies.Order.EditPrePayment` |
| **Edit Order (Post-Payment)** | Show "Edit" action, but clicking opens a **Supervisor Override PIN Dial**. | `Policies.Order.EditPostPayment` |
| **Void Order (Cancel Unpaid)** | Show "Void" button, but clicking opens a **Supervisor Override PIN Dial**. | `Policies.Order.Void` |
| **Delete Order (Wipe)** | Completely hide "Delete" action from the context menu. | `Policies.Order.HardDelete` |
| **Reprint Invoice/Receipt** | Hide "Reprint" button if not allowed. | `Policies.Order.Reprint` |
| **Shift Close** | Disable "Close Shift" button if not allowed. | `Policies.Shift.Close` |
| **Stock Adjustment** | Hide "Inventory Adjustment" module from side navigation. | `Policies.Inventory.Adjust` |
| **Report Access** | Hide "Reports" tab from app navigation. | `Policies.Reports.View` |

## 4. Implementation Guidelines for Sprint 2 (Frontend & Backend)

1. **Frontend (UI Guards)**: 
   - Role-based hiding must use a standardized component (e.g., `<RequirePolicy policy="Policies.Order.HardDelete">...</RequirePolicy>`).
   - For `Approval-Required` actions, the UI component should emit an `OnOverrideRequested` event which pops up the PIN modal before dispatching the GraphQL mutation.
2. **Backend (API Guards)**: 
   - Every GraphQL Mutation and Query listed above MUST have the corresponding `[Authorize(Policy = Policies.X)]` attribute applied.
   - For `Approval-Required` endpoints, the API requires the overriding user's token or the override PIN to be passed in the payload (e.g., mutation input contains optional `overridePin`).

## 5. Signoff

- [ ] PM / BA Signoff
- [ ] Tech Lead Signoff

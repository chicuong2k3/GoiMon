# UI Authorization Guard Pattern

**Related Story:** S1-04 Policy skeleton implementation (API + UI guard)

This document outlines the standard pattern for restricting UI elements based on the defined `role-permission-matrix.md`.

## 1. Policy Context Provider (React / Blazor)

The Client application must maintain the authenticated user's claims (specifically their role and specific policy grants mapped from the JWT token). 

```tsx
// Example using React Context abstraction
const { user, hasPolicy } = useAuthorization();
```

## 2. Policy-Based Render Component (`<RequirePolicy>`)

For simple Allow/Deny actions, developers must use the `<RequirePolicy>` wrapper component.

### Example: Hard Delete Button
The Matrix specifies that `Delete Order` is only allowed for `Policies.Order.HardDelete`.

**Incorrect Approach:**
```tsx
{user.role === 'Manager' || user.role === 'Owner' ? (
  <Button onClick={deleteOrder}>Delete</Button>
) : null}
```

**Correct Approach (Use Policy String):**
```tsx
<RequirePolicy policy={Policies.Order.HardDelete}>
  <Button onClick={deleteOrder}>Delete</Button>
</RequirePolicy>
```

If the user lacks the policy, the component will either not render or render a disabled version of the button (depending on the `<RequirePolicy>` fallback prop).

## 3. Approval-Required Guard Pattern

For actions defined as `Approval-Required` (e.g., Cashier attempting an `EditPostPayment` or `Void`), we need to show the UI element but intercept the action to prompt for a Supervisor PIN.

### The Component: `<ActionWithOverride>`

For actions requiring an override, developers wrap the action and handle the `onOverrideProvided` callback.

```tsx
<ActionWithOverride 
  policy={Policies.Order.Void}
  onAction={async (overrideToken) => {
    // The overrideToken is appended to the GraphQL mutation payload
    await voidOrderMutation({ variables: { orderId, overrideToken } });
  }}
>
  <Button>Void Order</Button>
</ActionWithOverride>
```

### Flow
1. User clicks the "Void Order" button.
2. `<ActionWithOverride>` checks the current user's role against `Policies.Order.Void`.
3. If allowed, `onAction(null)` is called directly.
4. If not allowed, a "Supervisor Override PIN" modal opens.
5. Supervisor types their PIN, which the Client exchanges for a temporary override token.
6. The override token is returned and `onAction(overrideToken)` is fired.

## 4. API Endpoints for UI

To support UI rendering logic without duplicating role logic, the Client can query its allowed policies:
- A GraphQL query `me { allowedPolicies }` returns a list of policy strings the user currently has, so the UI can quickly evaluate `RequirePolicy` offline.

## 5. Summary

- **Never hardcode roles in UI logic**. Always check against policy strings (`Policies.X`).
- **Use `<RequirePolicy>`** to hide or disable components entirely.
- **Use `<ActionWithOverride>`** for `Approval-Required` functions that might be overridden in real-time.

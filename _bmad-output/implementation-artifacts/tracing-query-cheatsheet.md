# 🔍 Tracing Query Cheat-Sheet (Seq)

This document provides standardized Seq query recipes for tracing requests across the GoiMon POS system using Correlation IDs.

---

## 1. Trace a Single Request
To see every log entry related to a specific customer interaction, filter by the `CorrelationId`.

**Seq Query:**
```sql
CorrelationId = "your-correlation-id-here"
```

---

## 2. Follow an Order Throughout Its Lifecycle
Tracing an order from creation to sync.

**Seq Query:**
```sql
@Message like "%Order created%" or @Message like "%Sync event%" and CorrelationId = "id-from-ui"
```

---

## 3. Find Requests with Errors by Correlation
Identify the specific request that caused an error and see the preceding logs.

**Steps:**
1. Find the error log.
2. Note its `CorrelationId`.
3. Run: `CorrelationId = "noted-id"`

---

## 4. Standard Correlation Field
Always ensure you are looking for the `CorrelationId` property (standardized in `CorrelationIdMiddleware`).

| Property Name | Description |
|---|---|
| `CorrelationId` | The unique ID for the request flow (UUID) |

---

## 5. Support Tips
- If a client reports an issue, ask for the `X-Correlation-Id` header from their browser's Network tab.
- Most "Internal Server Error" responses will include this header.

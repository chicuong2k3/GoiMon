# Sync Envelope Specification (device → server)

Purpose: deterministic, idempotent, tenant-scoped operation envelope for offline-first sync between device local store (EasyAppDev.Blazor.Store) and server.

1) Envelope (JSON)

```json
{
  "tenant_id": "<uuid>",
  "device_id": "<uuid>",
  "operation_id": "<uuid-v4-or-client-generated>",
  "operation_type": "order:create",
  "entity": "order",
  "payload": { /* entity data, may contain temporary client ids */ },
  "timestamp": "2026-02-26T10:00:00Z",
  "client_version": "1.0.0"
}
```

Fields
- tenant_id: tenant UUID (required)
- device_id: device UUID (optional but recommended)
- operation_id: client-generated idempotency key (required)
- operation_type: semantic type used by server to route handler
- payload: JSON object representing the entity; client may include `temp_id` fields
- timestamp: client event time for ordering/timestamps

2) Server response

```json
{
  "operation_id": "<client-op-id>",
  "status": "applied|conflict|rejected",
  "server_ids": { "order_id": "<server-uuid>" },
  "conflict": { /* optional conflict details */ },
  "applied_at": "2026-02-26T10:00:01Z"
}
```

3) Conflict and id mapping
- Client should send temporary ids for new entities (e.g., `temp_id`), server returns `server_ids` map so client can replace local ids.
- Idempotency: server uses `(tenant_id, operation_id)` to detect duplicates and return same ack.
- Conflict policy (MVP): last-write-wins by `timestamp` for simple fields; for orders prefer append-only — reject conflicting edits to paid orders.

4) Sync flow (device)
- Queue operation in local store with `operation_id` and payload.
- Attempt to push queued operations to server with tenant auth (sync token or JWT containing tenant_id).
- On success map returned `server_ids` to local store and mark operation applied.
- On conflict present minimal conflict details to user or log and follow policy.

5) Security
- Always authenticate sync requests; include device sync key signed in headers. Do not accept unauthenticated tenant_id.
- Rate-limit and validate payload sizes.

6) Example: create order payload

```json
{
  "tenant_id": "11111111-aaaa-1111-aaaa-111111111111",
  "device_id": "22222222-bbbb-2222-bbbb-222222222222",
  "operation_id": "33333333-cccc-3333-cccc-333333333333",
  "operation_type": "order:create",
  "entity": "order",
  "payload": {
    "temp_id": "tmp-1",
    "items": [ { "item_id": "5f3e...", "qty": 2, "unit_price_cents": 500 } ],
    "total_cents": 1000,
    "payment_method": "cash"
  },
  "timestamp": "2026-02-26T10:00:00Z"
}
```

7) Operational notes
- Keep sync messages small; prefer entity references rather than huge payloads.
- For large batches, send compressed payloads or break into chunks.

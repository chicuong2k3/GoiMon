# Sync Protocol — GoiMon (Draft)

Status: draft
Authors: Copilot on behalf of Chicuong
Date: 2026-02-26

Purpose
- Define the client↔server sync protocol used to upload queued local operations from the client (IndexedDB) to the server, ensure idempotent processing, and provide clear conflict-resolution behavior.
- Keep the spec concise (1–2 pages) so implementation can proceed with minimal ambiguity.

Scope
- Client-originated operations (create/update/cancel orders, payments markers, menu updates) that are queued locally when offline.
- Batch upload endpoint(s), response contract, error codes, and standard conflict resolution rules.

Principles
- Idempotency: Every client operation must include an `operationId` (UUIDv4) to allow the server to deduplicate retries.
- Batching: Clients should upload operations in ordered batches; each batch contains operations with sequential local sequence numbers.
- Minimal server responses: Server returns per-operation results and a batch-level status.
- Conflict first-class: Server returns explicit conflict results with suggested resolution hints.

Entities
- operationId: string (UUID) — unique per client operation
- clientId: string — device identifier (device-bound session)
- localSeq: integer — local per-client monotonically-increasing sequence number
- timestamp: ISO 8601 UTC string
- correlationId: string — optional client-supplied correlation id for grouping (e.g., order flow)

Endpoints
1) POST /api/sync/batch
- Request: JSON
  {
    "clientId": "<client-id>",
    "batchId": "<uuid>",
    "operations": [
      {
        "operationId": "<uuid>",
        "localSeq": 123,
        "type": "OrderCreate|OrderUpdate|OrderCancel|MenuUpdate|PaymentMark",
        "payload": { ... },
        "timestamp": "2026-02-26T15:00:00Z"
      },
      ...
    ]
  }

- Response: JSON (200)
  {
    "batchId": "<uuid>",
    "results": [
      { "operationId":"<uuid>", "status":"applied|conflict|rejected", "serverId":"<server-order-id|null>", "error": {"code":"", "message":""}, "conflict": {"type":"version_mismatch","serverState":{...},"resolutionHint":"accept_server|merge_client"} },
      ...
    ],
    "nextAction": "ok|retry|manual_resolution_required"
  }

- Error responses: standard HTTP codes and minimal JSON error payload: {"error":{"code":"SYNC_001","message":"Invalid payload"}}

Idempotency & Deduplication
- The server stores processed `operationId` per `clientId` for a retention window (e.g., 30 days) and returns the previous result if the `operationId` is re-sent.
- Clients must not assume success until they receive an `applied` result for each operation.
- Retries: clients re-send the same `operationId` on retry; server must be idempotent.

Conflict Resolution
- Types:
  - version_mismatch: payload is based on stale server state (e.g., updating an order already modified).
  - resource_not_found: client attempted to modify a deleted entity.
- Server returns `conflict` status and includes `serverState` and a `resolutionHint`.
- ResolutionHints:
  - accept_server: server state is authoritative; client should pull latest and update UI.
  - merge_client: server provides guidance for a merge or server applies a merge and returns merged state.
  - manual: requires human reconciliation; surface to owner.

Sync Ordering & Guarantees
- Batches preserve localSeq ordering per client. Server should process operations in ascending `localSeq` per client to maintain causal order where possible.
- The server may accept out-of-order operations but must return conflict/rejection when causality is violated.

Retries & Backoff
- Client should implement exponential backoff with jitter for network failures. For HTTP 429 or transient 5xx, wait and retry.
- For conflict responses, follow `resolutionHint`: if `accept_server` then pull latest state; if `merge_client` re-send merged operation; if `manual`, surface to user.

Security
- All sync endpoints must require TLS and a valid device-bound session token.
- Include `clientId` bound to the session during pairing; server should reject batches from unknown/unpaired `clientId`.

Audit & Observability
- Each processed operation must be logged with `operationId`, `clientId`, `localSeq`, `status`, and `processingLatency`.
- Metrics: operations/sec, conflicts/sec, retries/sec, average processing latency.

Examples
- OrderCreate example payload (inside operations[].payload):
  {
    "order": { "clientTempId":"tmp-1","items":[{"menuItemId":"m123","qty":2}], "total": 120000 }
  }

- Conflict response example for OrderUpdate:
  { "operationId":"op-1","status":"conflict","conflict":{"type":"version_mismatch","serverState":{"id":"s-1","items":[...]},"resolutionHint":"accept_server"} }

Retention
- Server stores processed `operationId` for at least 30 days to support idempotency across retries and client replays.

Open Items (to finalize)
- Exact storage schema for operation metadata and retention policy.
- Wire-format for `serverState` in conflict responses (consider using DTOs from Shared/ project).
- Sizing guidance for batch limits (e.g., max 100 operations or 1MB payload).



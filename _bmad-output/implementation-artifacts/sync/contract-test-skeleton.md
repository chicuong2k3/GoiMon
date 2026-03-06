# Sync Contract Test - Skeleton

Purpose: provide a minimal, CI-friendly contract-test specification for the sync event envelope described in ADR-005. Tests are intended to run as API-level contract checks and validate the server schema/behavior used by offline clients.

Test runner recommendation
- Host: `tests/GoiMon.Api.Tests` (xUnit or NUnit)
- Invocation: `dotnet test --filter Category=SyncContract`

Test cases (skeleton)
- Contract: required fields present
  - Arrange: a sample `sync-event` JSON (see sample below)
  - Act: call POST `/api/sync/events/validate` (or schema-registry endpoint) or validate payload against JSON Schema
  - Assert: response 200 + schemaVersion matches `1.0` and required fields exist

- Idempotency: duplicate event must be accepted but have no duplicate side-effect
  - Arrange: send same `eventId` twice
  - Act: replay event twice
  - Assert: second processing is a no-op (idempotent) or returns 2xx with dedupe result

- Tenant enforcement: TenantId required and scoped
  - Arrange: missing `tenantId` and invalid `tenantId`
  - Assert: requests without `tenantId` receive `4xx` and valid tenant is enforced

- Replay / ordering: out-of-order versions are handled gracefully
  - Arrange: send version 3 before version 2 for same aggregate
  - Assert: server either queues, rejects, or provides clear conflict response per ADR-005 rules

Sample event (use in tests)
```json
{
  "eventId": "0a1b2c3d-uuid",
  "aggregateId": "product-123",
  "eventType": "product.updated.v1",
  "version": 3,
  "tenantId": "tenant-xyz",
  "timestampUtc": "2026-03-07T12:00:00Z",
  "actor": { "id": "employee-1", "roles": ["cashier"] },
  "payload": { "name": "Updated Product Name", "price": 9.99 },
  "idempotencyKey": "op-456"
}
```

CI wiring notes
- Add a test job step `sync-contract-tests` which targets the API (running in test container or test host) and runs `dotnet test --filter Category=SyncContract`.
- Fail the pipeline on schema mismatch or missing required fields.

Next steps for implementers
- Create a lightweight test project or add tests under `tests/GoiMon.Api.Tests/Features/Sync` that implement the skeleton above.
- Optionally publish a JSON Schema to the schema-registry endpoint and validate payloads against it in tests.

# Tenancy artifacts — onboarding & tokens

This document explains quick onboarding and token generation for the multi-tenant shared-schema approach used by GoiMon.

1) Create tenant record (server-side)

- Insert tenant into `tenants` with name and plan. Capture `tenant_id` (UUID).

Example (Postgres):

```sql
INSERT INTO tenants (name, plan) VALUES ('Pho Lan', 'starter') RETURNING tenant_id;
```

2) Generate merchant JWT (server-side)

- For normal API usage, generate a short-lived JWT that includes a `tenant_id` claim and standard `sub`/`aud` claims. Sign with your server key.
- Example claims payload:

```json
{
  "sub": "merchant:123",
  "tenant_id": "<tenant-uuid>",
  "iat": 1670000000,
  "exp": 1670003600
}
```

3) Create device sync key (for offline devices)

- Generate an opaque random token for each device (store in `devices.sync_key`). Use this token to authenticate device-sync requests. The device includes the token in an `Authorization: Bearer <sync-key>` header when calling sync endpoints.

4) Static QR / public ordering link

- For customer-facing ordering links where no authentication is required, generate a short-lived read-only token (or embed `tenant_id` + one-way signature) in the QR so the static page can fetch tenant menu without exposing write access.

5) Recommended middleware usage

- Add `TenantMiddleware` early in pipeline (after authentication) so `HttpContext.Items["TenantId"]` is populated for downstream handlers and GraphQL interceptors.

Program.cs (pseudo):

```csharp
app.UseAuthentication();
app.UseTenantResolution();
app.MapGraphQL();
```

6) Sync flow summary

- Devices queue operations locally and push envelopes (see `sync-envelope.md`) with device sync key.
- Server validates token, extracts tenant, writes to `sync_queue`, applies idempotent processing, and returns mapping ack.

7) Security notes

- Never accept unauthenticated `tenant_id` from clients for write operations. Always validate sync key or JWT.
- Rotate sync keys when a device is lost.

# GoiMon Telemetry Metric Catalog

**Related Story:** S1-07 — Telemetry + dashboard baseline
**Status:** Done
**Platform:** .NET System.Diagnostics.Metrics + Serilog → Seq (`http://45.115.16.61:5341`)

---

## 1. Metric Naming Convention

```
goimon.<channel>.<event_name>
```

**Channels:** `orders`, `sync`, `payment`, `print`

All metrics are **Counters** (monotonically increasing). Tags provide segmentation axes.

---

## 2. Metric Catalog

### 2.1 Orders Channel (GoiMon.Api.Orders, v1.0)

| Metric Name | Tags | Description | Alert Threshold |
|---|---|---|---|
| `goimon.orders.config.created` | `line_count` | An order was successfully created | — |
| `goimon.orders.config.validation_failed` | `line_count`, `error_count` | Order input failed validation | > 10/min → Warn |
| `goimon.orders.config.selected_modifiers` | `line_count` | Modifier selections across created orders | — |

### 2.2 Sync / Outbox Channel (GoiMon.Api.PosOps, v1.0)

| Metric Name | Tags | Description | Alert Threshold |
|---|---|---|---|
| `goimon.sync.queued` | `event_type` | Sync event written to outbox | — |
| `goimon.sync.processed` | `event_type` | Sync event successfully dispatched | — |
| `goimon.sync.failed` | `event_type`, `error_category` | Sync dispatch failed (retriable) | > 5/min → Warn |
| `goimon.sync.dead_letter` | `event_type` | Event exceeded max retries (10) | Any → Error |

**Error categories** (`error_category` tag values):
- `TypeResolutionFailed` — event type name cannot be resolved to a CLR type
- `DeserializationNull` — JSON deserialized to null
- `<ExceptionTypeName>` — raw .NET exception type name (e.g. `TimeoutException`)

### 2.3 Payment Channel (GoiMon.Api.PosOps, v1.0)

| Metric Name | Tags | Description | Alert Threshold |
|---|---|---|---|
| `goimon.payment.initiated` | `provider` | Payment transaction started | — |
| `goimon.payment.succeeded` | `provider` | Payment completed successfully | — |
| `goimon.payment.failed` | `provider`, `error_code` | Payment transaction failed | Any → Error |

**Provider tag values:** `vnpay`, `momo`, `cash`, `other`

### 2.4 Print Channel (GoiMon.Api.PosOps, v1.0)

| Metric Name | Tags | Description | Alert Threshold |
|---|---|---|---|
| `goimon.print.requested` | `document_type` | Print job submitted | — |
| `goimon.print.succeeded` | `document_type` | Print job completed successfully | — |
| `goimon.print.failed` | `document_type`, `error_category` | Print job failed | > 3/min → Warn |

**Document type tag values:** `invoice`, `receipt`, `kitchen_ticket`, `report`

---

## 3. Alert Rules in Seq

Since the platform is **Seq** (not Prometheus/Grafana), alerts are configured through **Seq Signals + Alerts** on structured log properties. The `PosOperationTelemetry` service emits both counter increments (for future OTel export) and matching Serilog log events for immediate Seq alerting.

### Alert Configurations (Seq Signal Filters)

| Alert Name | Seq Filter | Severity | Notification |
|---|---|---|---|
| **Sync Dead-Letter** | `@Message like '%dead-lettered%' and SourceContext = 'GoiMon.Api.Infrastructure.Telemetry.PosOperationTelemetry'` | Error | PagerDuty / Email |
| **High Sync Failure Rate** | `@Message like '%Sync event failed%' and SourceContext = 'GoiMon.Api.Infrastructure.Telemetry.PosOperationTelemetry'` (> 5 in 1 min) | Warning | Slack |
| **Payment Failed** | `@Message like '%Payment failed%' and SourceContext = 'GoiMon.Api.Infrastructure.Telemetry.PosOperationTelemetry'` | Error | PagerDuty / Email |
| **High Print Failure Rate** | `@Message like '%Print failed%' and SourceContext = 'GoiMon.Api.Infrastructure.Telemetry.PosOperationTelemetry'` (> 3 in 1 min) | Warning | Slack |
| **High Order Validation Failure** | `SourceContext = 'GoiMon.Api.Features.Orders.Services.OrderTelemetry' and @Level = 'Warning'` (> 10 in 1 min) | Warning | Slack |

### Setting Up a Seq Alert (Steps)

1. Open Seq at `http://45.115.16.61:5341`
2. Navigate to **Signals → New Signal**
3. Enter the filter expression from the table above
4. Click **Alerts** tab → **Add Alert**
5. Set notification channel (email/webhook) and rate threshold
6. Save

---

## 4. Instrumentation Status

| Channel | Instrumented? | Notes |
|---|---|---|
| Orders | ✅ Yes | `OrderTelemetry` — validation + created counters |
| Sync (Outbox) | ✅ Yes | `OutboxService` instrumented via `PosOperationTelemetry` |
| Payment | ⚠️ Scaffolded | `PosOperationTelemetry` interface ready; payment adapter not yet built (Sprint 2) |
| Print | ⚠️ Scaffolded | `PosOperationTelemetry` interface ready; print adapter not yet built (Sprint 2) |

---

## 5. Future: OpenTelemetry Export

When a Prometheus/Grafana stack is available, add this package and config to enable OTLP export:

```xml
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.x.x" />
```

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(b => b
        .AddMeter("GoiMon.Api.Orders")
        .AddMeter("GoiMon.Api.PosOps")
        .AddPrometheusExporter());
```

This will make all counters in this catalog available as Prometheus metrics at `/metrics`.

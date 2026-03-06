using System.Diagnostics.Metrics;

namespace GoiMon.Api.Infrastructure.Telemetry;

/// <summary>
/// Implements POS operation telemetry using .NET Meters (compatible with OpenTelemetry exporters).
/// Also emits structured Serilog log entries for Seq alerting.
/// </summary>
public sealed class PosOperationTelemetry : IPosOperationTelemetry
{
    // -----------------------------------------------------------------------
    // Meter + Instruments
    // -----------------------------------------------------------------------
    private static readonly Meter Meter = new("GoiMon.Api.PosOps", "1.0");

    // Sync / Outbox
    private static readonly Counter<long> SyncQueued     = Meter.CreateCounter<long>("goimon.sync.queued",     description: "Total sync events enqueued to the outbox.");
    private static readonly Counter<long> SyncProcessed  = Meter.CreateCounter<long>("goimon.sync.processed",  description: "Total sync events successfully dispatched.");
    private static readonly Counter<long> SyncFailed     = Meter.CreateCounter<long>("goimon.sync.failed",     description: "Total sync event dispatch failures.");
    private static readonly Counter<long> SyncDeadLetter = Meter.CreateCounter<long>("goimon.sync.dead_letter",description: "Total sync events moved to dead-letter (max retries exceeded).");

    // Payment
    private static readonly Counter<long> PaymentInitiated = Meter.CreateCounter<long>("goimon.payment.initiated", description: "Total payment transactions initiated.");
    private static readonly Counter<long> PaymentSucceeded = Meter.CreateCounter<long>("goimon.payment.succeeded", description: "Total payment transactions completed successfully.");
    private static readonly Counter<long> PaymentFailed    = Meter.CreateCounter<long>("goimon.payment.failed",    description: "Total payment transactions that failed.");

    // Print
    private static readonly Counter<long> PrintRequested = Meter.CreateCounter<long>("goimon.print.requested", description: "Total print jobs requested.");
    private static readonly Counter<long> PrintSucceeded = Meter.CreateCounter<long>("goimon.print.succeeded", description: "Total print jobs completed successfully.");
    private static readonly Counter<long> PrintFailed    = Meter.CreateCounter<long>("goimon.print.failed",    description: "Total print jobs that failed.");

    // -----------------------------------------------------------------------
    private readonly ILogger<PosOperationTelemetry> _logger;

    public PosOperationTelemetry(ILogger<PosOperationTelemetry> logger)
    {
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // Sync / Outbox
    // -----------------------------------------------------------------------

    public void TrackSyncEventQueued(string eventType)
    {
        SyncQueued.Add(1, Tag("event_type", eventType));
        _logger.LogDebug("[Telemetry] Sync event queued. event_type={EventType}", eventType);
    }

    public void TrackSyncEventProcessed(string eventType, int attemptCount)
    {
        SyncProcessed.Add(1, Tag("event_type", eventType));
        _logger.LogInformation(
            "[Telemetry] Sync event processed. event_type={EventType} attempt_count={AttemptCount}",
            eventType, attemptCount);
    }

    public void TrackSyncEventFailed(string eventType, int attemptCount, string errorCategory)
    {
        SyncFailed.Add(1,
            Tag("event_type", eventType),
            Tag("error_category", errorCategory));
        _logger.LogWarning(
            "[Telemetry] Sync event failed. event_type={EventType} attempt_count={AttemptCount} error_category={ErrorCategory}",
            eventType, attemptCount, errorCategory);
    }

    public void TrackSyncDeadLettered(string eventType, int attemptCount)
    {
        SyncDeadLetter.Add(1, Tag("event_type", eventType));
        _logger.LogError(
            "[Telemetry] Sync event dead-lettered after {AttemptCount} attempts. event_type={EventType}",
            attemptCount, eventType);
    }

    // -----------------------------------------------------------------------
    // Payment
    // -----------------------------------------------------------------------

    public void TrackPaymentInitiated(string provider)
    {
        PaymentInitiated.Add(1, Tag("provider", provider));
        _logger.LogInformation("[Telemetry] Payment initiated. provider={Provider}", provider);
    }

    public void TrackPaymentSucceeded(string provider)
    {
        PaymentSucceeded.Add(1, Tag("provider", provider));
        _logger.LogInformation("[Telemetry] Payment succeeded. provider={Provider}", provider);
    }

    public void TrackPaymentFailed(string provider, string errorCode)
    {
        PaymentFailed.Add(1,
            Tag("provider", provider),
            Tag("error_code", errorCode));
        _logger.LogError(
            "[Telemetry] Payment failed. provider={Provider} error_code={ErrorCode}",
            provider, errorCode);
    }

    // -----------------------------------------------------------------------
    // Print
    // -----------------------------------------------------------------------

    public void TrackPrintRequested(string documentType)
    {
        PrintRequested.Add(1, Tag("document_type", documentType));
        _logger.LogDebug("[Telemetry] Print requested. document_type={DocumentType}", documentType);
    }

    public void TrackPrintSucceeded(string documentType)
    {
        PrintSucceeded.Add(1, Tag("document_type", documentType));
        _logger.LogInformation("[Telemetry] Print succeeded. document_type={DocumentType}", documentType);
    }

    public void TrackPrintFailed(string documentType, string errorCategory)
    {
        PrintFailed.Add(1,
            Tag("document_type", documentType),
            Tag("error_category", errorCategory));
        _logger.LogError(
            "[Telemetry] Print failed. document_type={DocumentType} error_category={ErrorCategory}",
            documentType, errorCategory);
    }

    // -----------------------------------------------------------------------
    // Helper
    // -----------------------------------------------------------------------
    private static KeyValuePair<string, object?> Tag(string key, object? value) => new(key, value);
}

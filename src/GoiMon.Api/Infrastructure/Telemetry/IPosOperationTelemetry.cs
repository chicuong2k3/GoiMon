namespace GoiMon.Api.Infrastructure.Telemetry;

/// <summary>
/// Central observability interface for POS key operation channels:
/// sync queue, payment, and print failures.
/// </summary>
public interface IPosOperationTelemetry
{
    // --- Sync / Outbox Channel ---
    void TrackSyncEventQueued(string eventType);
    void TrackSyncEventProcessed(string eventType, int attemptCount);
    void TrackSyncEventFailed(string eventType, int attemptCount, string errorCategory);
    void TrackSyncDeadLettered(string eventType, int attemptCount);

    // --- Payment Channel ---
    void TrackPaymentInitiated(string provider);
    void TrackPaymentSucceeded(string provider);
    void TrackPaymentFailed(string provider, string errorCode);

    // --- Print Channel ---
    void TrackPrintRequested(string documentType);
    void TrackPrintSucceeded(string documentType);
    void TrackPrintFailed(string documentType, string errorCategory);
}

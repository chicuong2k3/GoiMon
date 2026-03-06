using GoiMon.Api.Infrastructure.Data;
using GoiMon.Api.Domain.Events;
using GoiMon.Api.Infrastructure.Telemetry;

namespace GoiMon.Api.Infrastructure.Outbox;

/// <summary>
/// OutboxService exposes a single method `ProcessPendingAsync` which can be invoked
/// by an external scheduler/background library (e.g. Hangfire, Quartz.NET) that provides a dashboard.
/// </summary>
public class OutboxService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IPosOperationTelemetry _telemetry;

    private const int MaxAttempts = 10;

    public OutboxService(
        IDbContextFactory<AppDbContext> factory,
        IDomainEventDispatcher dispatcher,
        IPosOperationTelemetry telemetry)
    {
        _factory = factory;
        _dispatcher = dispatcher;
        _telemetry = telemetry;
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        using var db = _factory.CreateDbContext();

        var pending = await db.Set<OutboxEvent>()
            .Where(o => !o.Processed && o.AttemptCount < MaxAttempts)
            .OrderBy(o => o.OccurredOn)
            .Take(50)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var outbox in pending)
        {
            var eventType = outbox.TypeName?.Split('.').LastOrDefault() ?? "Unknown";

            try
            {
                // Try standard resolution first, then fall back to scanning the current assembly
                // (handles cases where TypeName is FullName without assembly qualification)
                var type = Type.GetType(outbox.TypeName!)
                    ?? typeof(OutboxService).Assembly.GetType(outbox.TypeName!);
                if (type is null)
                {
                    outbox.AttemptCount++;
                    outbox.LastError = $"Type resolution failed for '{outbox.TypeName}'";
                    _telemetry.TrackSyncEventFailed(eventType, outbox.AttemptCount, "TypeResolutionFailed");
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(outbox.Content, type);
                if (@event is null)
                {
                    outbox.AttemptCount++;
                    outbox.LastError = "Deserialization returned null";
                    _telemetry.TrackSyncEventFailed(eventType, outbox.AttemptCount, "DeserializationNull");
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                await _dispatcher.DispatchAsync(new[] { @event }, cancellationToken).ConfigureAwait(false);

                outbox.Processed = true;
                outbox.ProcessedOn = DateTimeOffset.UtcNow;
                outbox.AttemptCount++;
                outbox.LastError = null;

                _telemetry.TrackSyncEventProcessed(eventType, outbox.AttemptCount);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                outbox.AttemptCount++;
                outbox.LastError = ex.Message;

                if (outbox.AttemptCount >= MaxAttempts)
                    _telemetry.TrackSyncDeadLettered(eventType, outbox.AttemptCount);
                else
                    _telemetry.TrackSyncEventFailed(eventType, outbox.AttemptCount, ex.GetType().Name);

                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

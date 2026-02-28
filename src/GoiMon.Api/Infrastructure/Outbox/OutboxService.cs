using GoiMon.Api.Infrastructure.Data;
using GoiMon.Api.Domain.Events;

namespace GoiMon.Api.Infrastructure.Outbox;

/// <summary>
/// OutboxService exposes a single method `ProcessPendingAsync` which can be invoked
/// by an external scheduler/background library (e.g. Hangfire, Quartz.NET) that provides a dashboard.
/// </summary>
public class OutboxService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IDomainEventDispatcher _dispatcher;

    public OutboxService(IDbContextFactory<AppDbContext> factory, IDomainEventDispatcher dispatcher)
    {
        _factory = factory;
        _dispatcher = dispatcher;
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        using var db = _factory.CreateDbContext();

        var pending = await db.Set<OutboxEvent>()
            .Where(o => !o.Processed)
            .OrderBy(o => o.OccurredOn)
            .Take(50)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var outbox in pending)
        {
            try
            {
                var type = Type.GetType(outbox.TypeName!);
                if (type is null)
                {
                    outbox.AttemptCount++;
                    outbox.LastError = "Type resolution failed";
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(outbox.Content, type);
                if (@event is null)
                {
                    outbox.AttemptCount++;
                    outbox.LastError = "Deserialization returned null";
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                await _dispatcher.DispatchAsync(new[] { @event }, cancellationToken).ConfigureAwait(false);

                outbox.Processed = true;
                outbox.ProcessedOn = DateTimeOffset.UtcNow;
                outbox.AttemptCount++;
                outbox.LastError = null;
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                outbox.AttemptCount++;
                outbox.LastError = ex.Message;
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

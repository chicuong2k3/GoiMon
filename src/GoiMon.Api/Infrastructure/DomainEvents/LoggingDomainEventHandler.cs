using GoiMon.Api.Domain.Events;
using Microsoft.Extensions.Logging;

namespace GoiMon.Api.Infrastructure.DomainEvents;

public class LoggingDomainEventHandler<TEvent> : IDomainEventHandler<TEvent>
{
    private readonly ILogger<LoggingDomainEventHandler<TEvent>> _logger;

    public LoggingDomainEventHandler(ILogger<LoggingDomainEventHandler<TEvent>> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event handled: {Event}", @event?.ToString() ?? "<null>");
        return Task.CompletedTask;
    }
}

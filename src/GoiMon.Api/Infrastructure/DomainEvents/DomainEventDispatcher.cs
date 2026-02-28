using GoiMon.Api.Domain.Events;

namespace GoiMon.Api.Infrastructure.DomainEvents;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;

    public DomainEventDispatcher(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task DispatchAsync(IEnumerable<object> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            var eventType = @event.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlersObj = _provider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType));
            if (handlersObj is not System.Collections.IEnumerable handlers) continue;

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");
                if (method is null) continue;
                var task = (Task?)method.Invoke(handler, new object[] { @event, cancellationToken });
                if (task is not null) await task.ConfigureAwait(false);
            }
        }
    }
}

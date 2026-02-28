namespace GoiMon.Api.Domain.Events;

public interface IDomainEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

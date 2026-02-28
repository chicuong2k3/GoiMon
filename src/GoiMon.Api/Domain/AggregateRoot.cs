namespace GoiMon.Api.Domain;

/// <summary>
/// Base class for aggregate roots.
/// Provides an Id property and a simple domain-event collection helper.
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    protected AggregateRoot() { }

    protected AggregateRoot(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }

    private readonly List<object> _domainEvents = new();

    [GraphQLIgnore]
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(object @event)
    {
        if (@event is null) return;
        _domainEvents.Add(@event);
    }

    protected void RemoveDomainEvent(object @event)
    {
        if (@event is null) return;
        _domainEvents.Remove(@event);
    }

    protected void ClearDomainEvents() => _domainEvents.Clear();

    // Public helper for infrastructure to clear events after dispatch
    public void ClearEvents() => ClearDomainEvents();
}

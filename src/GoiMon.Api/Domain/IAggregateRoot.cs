namespace GoiMon.Api.Domain;

/// <summary>
/// Marker interface for aggregate roots in the domain model.
/// Implementing types are entity roots that define transactional boundaries.
/// </summary>
public interface IAggregateRoot
{
    Guid Id { get; }
}

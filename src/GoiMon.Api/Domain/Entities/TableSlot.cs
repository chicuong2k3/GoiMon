namespace GoiMon.Api.Domain.Entities;

public sealed class TableSlot : AggregateRoot
{
    private TableSlot() { }

    public TableSlot(Guid id, string code, string name, int capacity)
    {
        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Capacity = capacity;
        IsActive = true;
        CurrentState = Domain.Enums.TableServiceState.Available;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public bool IsActive { get; private set; }
    public Domain.Enums.TableServiceState CurrentState { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(string code, string name, int capacity)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Capacity = capacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetState(Domain.Enums.TableServiceState state)
    {
        CurrentState = state;
        UpdatedAt = DateTime.UtcNow;
    }
}

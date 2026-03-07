namespace GoiMon.Api.Infrastructure.Outbox;

public class OutboxEvent
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; } // Nullable if some events are global
    public string TypeName { get; set; } = null!; // assembly-qualified type name
    public string Content { get; set; } = null!; // JSON payload
    public DateTimeOffset OccurredOn { get; set; }
    public bool Processed { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}

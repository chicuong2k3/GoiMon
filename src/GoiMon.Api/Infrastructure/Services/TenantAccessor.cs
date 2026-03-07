namespace GoiMon.Api.Infrastructure.Services;

public class TenantAccessor : ITenantAccessor
{
    private static readonly AsyncLocal<Guid?> _current = new();

    public Guid? TenantId
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

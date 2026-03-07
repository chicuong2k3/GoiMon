using System.Linq.Expressions;
using System.Text.Json;

namespace GoiMon.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly GoiMon.Api.Infrastructure.Services.ITenantAccessor _accessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        GoiMon.Api.Infrastructure.Services.ITenantAccessor accessor)
        : base(options)
    {
        _accessor = accessor;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderItemModifier> OrderItemModifiers { get; set; } = null!;
    public DbSet<ProductCombo> ProductCombos { get; set; } = null!;
    public DbSet<ProductComboItem> ProductComboItems { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<ModifierGroup> ModifierGroups { get; set; } = null!;
    public DbSet<ModifierOption> ModifierOptions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<OtpToken> OtpTokens { get; set; } = null!;
    public DbSet<TableSlot> TableSlots { get; set; } = null!;
    public DbSet<Infrastructure.Outbox.OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Load all IEntityTypeConfiguration<> implementations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply Global Query Filters for Multi-tenant entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Domain.IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, "TenantId"),
                    Expression.Constant(_accessor.TenantId ?? Guid.Empty)
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenantId = _accessor.TenantId;

        // Auto-assign TenantId to new Multi-tenant entities
        foreach (var entry in ChangeTracker.Entries<Domain.IMultiTenant>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty && currentTenantId.HasValue)
                {
                    entry.Entity.TenantId = currentTenantId.Value;
                }
            }
        }

        // Collect domain events from tracked aggregate roots
        var domainEntities = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<Domain.AggregateRoot>()
            .Where(e => e.DomainEvents?.Any() == true)
            .ToList();

        var events = domainEntities.SelectMany(e => e.DomainEvents!).ToList();

        if (events.Any())
        {
            // Convert domain events to outbox records and persist them within the same transaction
            foreach (var @event in events)
            {
                var payload = JsonSerializer.Serialize(@event, @event.GetType());
                var outbox = new Infrastructure.Outbox.OutboxEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = currentTenantId,
                    TypeName = @event.GetType().FullName ?? @event.GetType().Name,
                    Content = payload,
                    OccurredOn = DateTimeOffset.UtcNow,
                    Processed = false,
                    AttemptCount = 0
                };
                OutboxEvents.Add(outbox);
            }
        }

        // Persist aggregates and outbox records atomically
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Clear domain events from aggregates after they have been persisted to the DB
        foreach (var ent in domainEntities)
            ent.ClearEvents();

        return result;
    }
}

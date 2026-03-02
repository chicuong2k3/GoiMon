using System.Diagnostics.Metrics;

namespace GoiMon.Api.Features.Orders.Services;

public class OrderTelemetry : IOrderTelemetry
{
    private static readonly Meter Meter = new("GoiMon.Api.Orders", "1.0");
    private static readonly Counter<long> ValidationFailedCounter = Meter.CreateCounter<long>("goimon.orders.config.validation_failed");
    private static readonly Counter<long> OrdersCreatedCounter = Meter.CreateCounter<long>("goimon.orders.config.created");
    private static readonly Counter<long> SelectedModifierCounter = Meter.CreateCounter<long>("goimon.orders.config.selected_modifiers");

    private readonly ILogger<OrderTelemetry> _logger;

    public OrderTelemetry(ILogger<OrderTelemetry> logger)
    {
        _logger = logger;
    }

    public void TrackValidationFailed(int errorCount, int lineCount)
    {
        ValidationFailedCounter.Add(1,
            new KeyValuePair<string, object?>("line_count", lineCount),
            new KeyValuePair<string, object?>("error_count", errorCount));

        _logger.LogWarning("Configured order validation failed. line_count={LineCount}, error_count={ErrorCount}", lineCount, errorCount);
    }

    public void TrackOrderCreated(int lineCount, int selectedModifierCount, decimal total)
    {
        OrdersCreatedCounter.Add(1,
            new KeyValuePair<string, object?>("line_count", lineCount));

        SelectedModifierCounter.Add(selectedModifierCount,
            new KeyValuePair<string, object?>("line_count", lineCount));

        _logger.LogInformation(
            "Configured order created. line_count={LineCount}, selected_modifier_count={SelectedModifierCount}, total={Total}",
            lineCount,
            selectedModifierCount,
            total);
    }
}

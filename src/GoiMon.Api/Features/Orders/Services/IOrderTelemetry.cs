namespace GoiMon.Api.Features.Orders.Services;

public interface IOrderTelemetry
{
    void TrackValidationFailed(int errorCount, int lineCount);
    void TrackOrderCreated(int lineCount, int selectedModifierCount, decimal total);
}

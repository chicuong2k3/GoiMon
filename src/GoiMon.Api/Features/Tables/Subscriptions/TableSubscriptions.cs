namespace GoiMon.Api.Features.Tables.Subscriptions;

[ExtendObjectType("Subscription")]
public sealed class TableSubscriptions
{
    [Subscribe]
    [Topic(TableSubscriptionTopics.TableSlotChanged)]
    public TableSlot OnTableSlotChanged([EventMessage] TableSlot tableSlot) => tableSlot;
}

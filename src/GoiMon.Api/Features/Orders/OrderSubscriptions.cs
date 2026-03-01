using HotChocolate.Subscriptions;

namespace GoiMon.Api.Features.Orders;

[ExtendObjectType("Subscription")]
public class OrderSubscriptions
{
    [Subscribe]
    [Topic(OrderSubscriptionTopics.OrderChanged)]
    public Order OnOrderChanged([EventMessage] Order order) => order;
}

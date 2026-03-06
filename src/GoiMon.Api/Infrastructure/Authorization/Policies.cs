namespace GoiMon.Api.Infrastructure.Authorization;

public static class Policies
{
    public static class Order
    {
        public const string EditPrePayment = "Policies.Order.EditPrePayment";
        public const string EditPostPayment = "Policies.Order.EditPostPayment";
        public const string Void = "Policies.Order.Void";
        public const string HardDelete = "Policies.Order.HardDelete";
        public const string Reprint = "Policies.Order.Reprint";
    }

    public static class Shift
    {
        public const string Close = "Policies.Shift.Close";
    }

    public static class Inventory
    {
        public const string Adjust = "Policies.Inventory.Adjust";
    }

    public static class Reports
    {
        public const string View = "Policies.Reports.View";
    }
}

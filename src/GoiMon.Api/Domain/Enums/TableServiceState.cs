namespace GoiMon.Api.Domain.Enums;

public enum TableServiceState
{
    Available = 1,
    Occupied = 2,
    Preparing = 3,
    ReadyToServe = 4,
    AwaitingPayment = 5,
    Paid = 6
}

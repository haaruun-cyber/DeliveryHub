namespace DeliveryHub.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Assigned = 1,
    PickedUp = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5
}

namespace DeliveryHub.Application.DTOs.Orders;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid? DriverId,
    string? DriverName,
    string PickupLocation,
    string DeliveryLocation,
    double? PickupLatitude,
    double? PickupLongitude,
    double? DeliveryLatitude,
    double? DeliveryLongitude,
    decimal? DistanceKm,
    string OrderStatus,
    decimal DeliveryFee,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);

public record CreateOrderDto(
    Guid CustomerId,
    string PickupLocation,
    string DeliveryLocation,
    double? PickupLatitude,
    double? PickupLongitude,
    double? DeliveryLatitude,
    double? DeliveryLongitude,
    decimal? DistanceKm,
    decimal DeliveryFee);

public record UpdateOrderDto(
    string PickupLocation,
    string DeliveryLocation,
    double? PickupLatitude,
    double? PickupLongitude,
    double? DeliveryLatitude,
    double? DeliveryLongitude,
    decimal? DistanceKm,
    decimal DeliveryFee,
    string OrderStatus);
public record AssignDriverDto(Guid DriverId);
public record UpdateOrderStatusDto(string OrderStatus);

namespace DeliveryHub.Application.DTOs.Tracking;

public record TrackingDto(
    Guid Id,
    Guid OrderId,
    string CurrentLocation,
    double? Latitude,
    double? Longitude,
    string TrackingStatus,
    DateTime UpdatedAt
);

public record CreateTrackingDto(Guid OrderId, string CurrentLocation, double? Latitude, double? Longitude, string TrackingStatus);
public record UpdateTrackingDto(string CurrentLocation, double? Latitude, double? Longitude, string TrackingStatus);

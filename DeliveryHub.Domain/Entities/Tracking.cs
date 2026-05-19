using DeliveryHub.Domain.Enums;

namespace DeliveryHub.Domain.Entities;

public class Tracking
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string CurrentLocation { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public TrackingStatus TrackingStatus { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

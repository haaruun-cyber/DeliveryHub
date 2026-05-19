namespace DeliveryHub.Application.DTOs.Drivers;

public record DriverDto(Guid Id, string FullName, string Phone, string VehicleType, string LicenseNumber, string Status, DateTime CreatedAt);
public record CreateDriverDto(string FullName, string Phone, string VehicleType, string LicenseNumber, string? Status);
public record UpdateDriverDto(string FullName, string Phone, string VehicleType, string LicenseNumber, string Status);

namespace DeliveryHub.Application.DTOs.Customers;

public record CustomerDto(Guid Id, string FullName, string Phone, string Address, string? Email, DateTime CreatedAt);
public record CreateCustomerDto(string FullName, string Phone, string Address, string? Email);
public record UpdateCustomerDto(string FullName, string Phone, string Address, string? Email);

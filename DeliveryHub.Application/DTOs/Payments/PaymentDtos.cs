namespace DeliveryHub.Application.DTOs.Payments;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    DateTime? PaidAt
);

public record CreatePaymentDto(Guid OrderId, decimal Amount, string PaymentMethod);
public record UpdatePaymentDto(string PaymentStatus, string? PaymentMethod);

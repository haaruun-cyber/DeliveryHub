using DeliveryHub.Application.DTOs.Customers;
using DeliveryHub.Application.DTOs.Drivers;
using DeliveryHub.Application.DTOs.Orders;
using DeliveryHub.Application.DTOs.Payments;
using DeliveryHub.Application.DTOs.Tracking;
using FluentValidation;

namespace DeliveryHub.Application.Validators;

public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
    }
}

public class CreateDriverDtoValidator : AbstractValidator<CreateDriverDto>
{
    public CreateDriverDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.VehicleType).NotEmpty();
        RuleFor(x => x.LicenseNumber).NotEmpty();
    }
}

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.PickupLocation).NotEmpty();
        RuleFor(x => x.DeliveryLocation).NotEmpty();
        RuleFor(x => x.PickupLatitude).InclusiveBetween(-90, 90).When(x => x.PickupLatitude.HasValue);
        RuleFor(x => x.PickupLongitude).InclusiveBetween(-180, 180).When(x => x.PickupLongitude.HasValue);
        RuleFor(x => x.DeliveryLatitude).InclusiveBetween(-90, 90).When(x => x.DeliveryLatitude.HasValue);
        RuleFor(x => x.DeliveryLongitude).InclusiveBetween(-180, 180).When(x => x.DeliveryLongitude.HasValue);
        RuleFor(x => x.DistanceKm).GreaterThanOrEqualTo(0).When(x => x.DistanceKm.HasValue);
        RuleFor(x => x.DeliveryFee).GreaterThanOrEqualTo(0);
    }
}

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}

public class CreateTrackingDtoValidator : AbstractValidator<CreateTrackingDto>
{
    public CreateTrackingDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CurrentLocation).NotEmpty();
        RuleFor(x => x.TrackingStatus).NotEmpty();
    }
}

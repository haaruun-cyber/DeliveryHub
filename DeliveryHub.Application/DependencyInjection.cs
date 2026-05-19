using DeliveryHub.Application.Interfaces;
using DeliveryHub.Application.Services;
using DeliveryHub.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}

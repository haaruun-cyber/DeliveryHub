using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Auth;
using DeliveryHub.Application.DTOs.Customers;
using DeliveryHub.Application.DTOs.Drivers;
using DeliveryHub.Application.DTOs.Orders;
using DeliveryHub.Application.DTOs.Payments;
using DeliveryHub.Application.DTOs.Reports;
using DeliveryHub.Application.DTOs.Tracking;

namespace DeliveryHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileImageAsync(Guid userId, UpdateProfileImageDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default);
    Task<string?> RequestPasswordResetAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
}

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllAsync(QueryParams query, CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IDriverService
{
    Task<PagedResult<DriverDto>> GetAllAsync(QueryParams query, string? status, CancellationToken ct = default);
    Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DriverDto> CreateAsync(CreateDriverDto dto, CancellationToken ct = default);
    Task<DriverDto?> UpdateAsync(Guid id, UpdateDriverDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetAllAsync(QueryParams query, string? status, Guid? customerId, Guid? driverId, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderDto dto, CancellationToken ct = default);
    Task<OrderDto?> AssignDriverAsync(Guid id, AssignDriverDto dto, CancellationToken ct = default);
    Task<OrderDto?> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetAllAsync(QueryParams query, string? status, CancellationToken ct = default);
    Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto, CancellationToken ct = default);
    Task<PaymentDto?> UpdateAsync(Guid id, UpdatePaymentDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITrackingService
{
    Task<PagedResult<TrackingDto>> GetAllAsync(QueryParams query, Guid? orderId, CancellationToken ct = default);
    Task<TrackingDto?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<List<TrackingDto>> GetHistoryByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<TrackingDto> CreateAsync(CreateTrackingDto dto, CancellationToken ct = default);
    Task<TrackingDto?> UpdateAsync(Guid id, UpdateTrackingDto dto, CancellationToken ct = default);
}

public interface IReportService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<List<DailyOrderStatDto>> GetDailyOrdersAsync(int days = 7, CancellationToken ct = default);
    Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months = 6, CancellationToken ct = default);
    Task<List<TopDriverDto>> GetTopDriversAsync(int top = 5, CancellationToken ct = default);
    Task<List<PaymentStatDto>> GetPaymentStatsAsync(CancellationToken ct = default);
    Task<List<DeliveryPerformanceDto>> GetDeliveryPerformanceAsync(CancellationToken ct = default);
}

public interface ITokenService
{
    AuthResponseDto GenerateToken(Guid userId, string fullName, string email, string role, string? profileImageUrl = null, Guid? customerId = null, Guid? driverId = null);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

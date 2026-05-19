namespace DeliveryHub.Application.DTOs.Reports;

public record DashboardStatsDto(
    int TotalOrders,
    int TotalCustomers,
    int TotalDrivers,
    decimal TotalRevenue,
    int PendingOrders,
    int DeliveredOrders,
    int ActiveDrivers
);

public record DailyOrderStatDto(string Date, int Count);
public record MonthlyRevenueDto(string Month, decimal Revenue);
public record TopDriverDto(Guid DriverId, string DriverName, int Deliveries);
public record PaymentStatDto(string PaymentMethod, int Count, decimal Total);
public record DeliveryPerformanceDto(string Status, int Count);

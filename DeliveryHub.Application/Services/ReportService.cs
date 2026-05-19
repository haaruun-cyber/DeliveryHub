using DeliveryHub.Application.DTOs.Reports;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Enums;

namespace DeliveryHub.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository) => _repository = repository;

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var statusCounts = await _repository.GetOrderStatusCountsAsync(ct);
        return new DashboardStatsDto(
            await _repository.CountOrdersAsync(ct),
            await _repository.CountCustomersAsync(ct),
            await _repository.CountDriversAsync(ct),
            await _repository.GetTotalRevenueAsync(ct),
            statusCounts.GetValueOrDefault(OrderStatus.Pending.ToString(), 0),
            statusCounts.GetValueOrDefault(OrderStatus.Delivered.ToString(), 0),
            await _repository.CountActiveDriversAsync(ct)
        );
    }

    public async Task<List<DailyOrderStatDto>> GetDailyOrdersAsync(int days = 7, CancellationToken ct = default)
    {
        var data = await _repository.GetDailyOrdersAsync(days, ct);
        return data.Select(d => new DailyOrderStatDto(d.Date, d.Count)).ToList();
    }

    public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months = 6, CancellationToken ct = default)
    {
        var data = await _repository.GetMonthlyRevenueAsync(months, ct);
        return data.Select(m => new MonthlyRevenueDto(m.Month, m.Revenue)).ToList();
    }

    public async Task<List<TopDriverDto>> GetTopDriversAsync(int top = 5, CancellationToken ct = default)
    {
        var data = await _repository.GetTopDriversAsync(top, ct);
        return data.Select(d => new TopDriverDto(d.DriverId, d.Name, d.Count)).ToList();
    }

    public async Task<List<PaymentStatDto>> GetPaymentStatsAsync(CancellationToken ct = default)
    {
        var data = await _repository.GetPaymentStatsAsync(ct);
        return data.Select(p => new PaymentStatDto(p.Method, p.Count, p.Total)).ToList();
    }

    public async Task<List<DeliveryPerformanceDto>> GetDeliveryPerformanceAsync(CancellationToken ct = default)
    {
        var counts = await _repository.GetOrderStatusCountsAsync(ct);
        return counts.Select(c => new DeliveryPerformanceDto(c.Key, c.Value)).ToList();
    }
}

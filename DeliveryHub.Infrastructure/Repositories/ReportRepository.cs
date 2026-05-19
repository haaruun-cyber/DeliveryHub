using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Enums;
using DeliveryHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryHub.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context) => _context = context;

    public Task<int> CountOrdersAsync(CancellationToken ct = default) =>
        _context.Orders.CountAsync(ct);

    public Task<int> CountCustomersAsync(CancellationToken ct = default) =>
        _context.Customers.CountAsync(ct);

    public Task<int> CountDriversAsync(CancellationToken ct = default) =>
        _context.Drivers.CountAsync(ct);

    public Task<int> CountActiveDriversAsync(CancellationToken ct = default) =>
        _context.Drivers.CountAsync(d => d.Status != DriverStatus.Offline, ct);

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default) =>
        await _context.Payments
            .Where(p => p.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

    public async Task<List<(string Date, int Count)>> GetDailyOrdersAsync(int days, CancellationToken ct = default)
    {
        var from = DateTime.UtcNow.Date.AddDays(-days + 1);
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= from)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Enumerable.Range(0, days)
            .Select(i =>
            {
                var date = from.AddDays(i);
                var count = orders.FirstOrDefault(o => o.Date == date)?.Count ?? 0;
                return (date.ToString("MMM dd"), count);
            })
            .ToList();
    }

    public async Task<List<(string Month, decimal Revenue)>> GetMonthlyRevenueAsync(int months, CancellationToken ct = default)
    {
        var from = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-months + 1);
        var paidPayments = await _context.Payments
            .Where(p => p.PaymentStatus == PaymentStatus.Paid && p.PaidAt != null && p.PaidAt >= from)
            .Select(p => new { p.PaidAt, p.Amount })
            .ToListAsync(ct);

        var revenueByMonth = paidPayments
            .GroupBy(p => (p.PaidAt!.Value.Year, p.PaidAt!.Value.Month))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        return Enumerable.Range(0, months)
            .Select(i =>
            {
                var d = from.AddMonths(i);
                var rev = revenueByMonth.TryGetValue((d.Year, d.Month), out var amount) ? amount : 0m;
                return (d.ToString("MMM yyyy"), rev);
            })
            .ToList();
    }

    public async Task<List<(Guid DriverId, string Name, int Count)>> GetTopDriversAsync(int top, CancellationToken ct = default)
    {
        var grouped = await _context.Orders
            .Where(o => o.DriverId != null && o.OrderStatus == OrderStatus.Delivered)
            .GroupBy(o => o.DriverId)
            .Select(g => new { DriverId = g.Key!.Value, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(ct);

        if (grouped.Count == 0)
            return [];

        var driverIds = grouped.Select(g => g.DriverId).ToList();
        var driverNames = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.FullName, ct);

        return grouped
            .Select(g => (g.DriverId, driverNames.GetValueOrDefault(g.DriverId, "Unknown"), g.Count))
            .ToList();
    }

    public async Task<List<(string Method, int Count, decimal Total)>> GetPaymentStatsAsync(CancellationToken ct = default)
    {
        var stats = await _context.Payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(p => p.Amount) })
            .ToListAsync(ct);

        return stats.Select(s => (s.Method.ToString(), s.Count, s.Total)).ToList();
    }

    public async Task<Dictionary<string, int>> GetOrderStatusCountsAsync(CancellationToken ct = default) =>
        await _context.Orders
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
}

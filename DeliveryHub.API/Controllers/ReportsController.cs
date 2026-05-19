using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Reports;
using DeliveryHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboard(CancellationToken ct) =>
        Ok(ApiResponse<DashboardStatsDto>.Ok(await _service.GetDashboardStatsAsync(ct)));

    [HttpGet("daily-orders")]
    public async Task<ActionResult<ApiResponse<List<DailyOrderStatDto>>>> GetDailyOrders([FromQuery] int days = 7, CancellationToken ct = default) =>
        Ok(ApiResponse<List<DailyOrderStatDto>>.Ok(await _service.GetDailyOrdersAsync(days, ct)));

    [HttpGet("monthly-revenue")]
    public async Task<ActionResult<ApiResponse<List<MonthlyRevenueDto>>>> GetMonthlyRevenue([FromQuery] int months = 6, CancellationToken ct = default) =>
        Ok(ApiResponse<List<MonthlyRevenueDto>>.Ok(await _service.GetMonthlyRevenueAsync(months, ct)));

    [HttpGet("top-drivers")]
    public async Task<ActionResult<ApiResponse<List<TopDriverDto>>>> GetTopDrivers([FromQuery] int top = 5, CancellationToken ct = default) =>
        Ok(ApiResponse<List<TopDriverDto>>.Ok(await _service.GetTopDriversAsync(top, ct)));

    [HttpGet("payment-stats")]
    public async Task<ActionResult<ApiResponse<List<PaymentStatDto>>>> GetPaymentStats(CancellationToken ct) =>
        Ok(ApiResponse<List<PaymentStatDto>>.Ok(await _service.GetPaymentStatsAsync(ct)));

    [HttpGet("delivery-performance")]
    public async Task<ActionResult<ApiResponse<List<DeliveryPerformanceDto>>>> GetDeliveryPerformance(CancellationToken ct) =>
        Ok(ApiResponse<List<DeliveryPerformanceDto>>.Ok(await _service.GetDeliveryPerformanceAsync(ct)));
}

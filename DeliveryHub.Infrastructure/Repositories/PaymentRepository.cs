using DeliveryHub.Application.Common;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;
using DeliveryHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryHub.Infrastructure.Repositories;

public class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Payment>> GetPagedAsync(QueryParams query, string? statusFilter, CancellationToken ct = default)
    {
        var q = DbSet.Include(p => p.Order).AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<PaymentStatus>(statusFilter, true, out var status))
            q = q.Where(p => p.PaymentStatus == status);

        q = query.SortDesc ? q.OrderByDescending(p => p.PaidAt) : q.OrderBy(p => p.PaidAt ?? DateTime.MinValue);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<Payment> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
}

using DeliveryHub.Application.Common;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryHub.Infrastructure.Repositories;

public class TrackingRepository : RepositoryBase<Tracking>, ITrackingRepository
{
    public TrackingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Tracking>> GetPagedAsync(QueryParams query, Guid? orderId, CancellationToken ct = default)
    {
        var q = DbSet.AsQueryable();
        if (orderId.HasValue) q = q.Where(t => t.OrderId == orderId);
        q = q.OrderByDescending(t => t.UpdatedAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<Tracking> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<Tracking?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await DbSet.Where(t => t.OrderId == orderId).OrderByDescending(t => t.UpdatedAt).FirstOrDefaultAsync(ct);

    public async Task<List<Tracking>> GetHistoryByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await DbSet.Where(t => t.OrderId == orderId).OrderByDescending(t => t.UpdatedAt).ToListAsync(ct);
}

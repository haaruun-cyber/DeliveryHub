using DeliveryHub.Application.Common;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;
using DeliveryHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryHub.Infrastructure.Repositories;

public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Order>> GetPagedAsync(QueryParams query, string? statusFilter, Guid? customerId, Guid? driverId, CancellationToken ct = default)
    {
        var q = DbSet.Include(o => o.Customer).Include(o => o.Driver).AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, true, out var status))
            q = q.Where(o => o.OrderStatus == status);
        if (customerId.HasValue) q = q.Where(o => o.CustomerId == customerId);
        if (driverId.HasValue) q = q.Where(o => o.DriverId == driverId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(o => o.PickupLocation.ToLower().Contains(s) ||
                             o.DeliveryLocation.ToLower().Contains(s) ||
                             o.Customer.FullName.ToLower().Contains(s));
        }

        q = query.SortBy?.ToLower() switch
        {
            "deliveryfee" => query.SortDesc ? q.OrderByDescending(o => o.DeliveryFee) : q.OrderBy(o => o.DeliveryFee),
            "orderstatus" => query.SortDesc ? q.OrderByDescending(o => o.OrderStatus) : q.OrderBy(o => o.OrderStatus),
            _ => q.OrderByDescending(o => o.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<Order> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Customer).Include(o => o.Driver).FirstOrDefaultAsync(o => o.Id == id, ct);
}

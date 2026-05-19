using DeliveryHub.Application.Common;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;
using DeliveryHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryHub.Infrastructure.Repositories;

public class DriverRepository : RepositoryBase<Driver>, IDriverRepository
{
    public DriverRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Driver>> GetPagedAsync(QueryParams query, string? statusFilter, CancellationToken ct = default)
    {
        var q = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<DriverStatus>(statusFilter, true, out var status))
            q = q.Where(d => d.Status == status);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(d => d.FullName.ToLower().Contains(s) || d.Phone.Contains(s) || d.LicenseNumber.Contains(s));
        }

        q = query.SortBy?.ToLower() switch
        {
            "fullname" => query.SortDesc ? q.OrderByDescending(d => d.FullName) : q.OrderBy(d => d.FullName),
            _ => q.OrderByDescending(d => d.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<Driver> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(d => d.UserId == userId, ct);

    public async Task<List<Driver>> GetAvailableAsync(CancellationToken ct = default) =>
        await DbSet.Where(d => d.Status == DriverStatus.Available).ToListAsync(ct);
}

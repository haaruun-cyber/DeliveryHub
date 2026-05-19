using DeliveryHub.Application.Common;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryHub.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Customer>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var q = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(c => c.FullName.ToLower().Contains(s) ||
                             c.Phone.Contains(s) ||
                             (c.Email != null && c.Email.ToLower().Contains(s)));
        }

        q = ApplySort(q, query);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<Customer> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    private static IQueryable<Customer> ApplySort(IQueryable<Customer> q, QueryParams query) =>
        query.SortBy?.ToLower() switch
        {
            "fullname" => query.SortDesc ? q.OrderByDescending(c => c.FullName) : q.OrderBy(c => c.FullName),
            "createdat" => query.SortDesc ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
            _ => q.OrderByDescending(c => c.CreatedAt)
        };
}

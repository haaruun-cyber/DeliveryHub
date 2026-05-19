using DeliveryHub.Application.Common;
using DeliveryHub.Domain.Entities;

namespace DeliveryHub.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<PagedResult<Customer>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}

public interface IDriverRepository : IRepository<Driver>
{
    Task<PagedResult<Driver>> GetPagedAsync(QueryParams query, string? statusFilter, CancellationToken ct = default);
    Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<Driver>> GetAvailableAsync(CancellationToken ct = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<PagedResult<Order>> GetPagedAsync(QueryParams query, string? statusFilter, Guid? customerId, Guid? driverId, CancellationToken ct = default);
    Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<PagedResult<Payment>> GetPagedAsync(QueryParams query, string? statusFilter, CancellationToken ct = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}

public interface ITrackingRepository : IRepository<Tracking>
{
    Task<PagedResult<Tracking>> GetPagedAsync(QueryParams query, Guid? orderId, CancellationToken ct = default);
    Task<Tracking?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<List<Tracking>> GetHistoryByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}

public interface IReportRepository
{
    Task<int> CountOrdersAsync(CancellationToken ct = default);
    Task<int> CountCustomersAsync(CancellationToken ct = default);
    Task<int> CountDriversAsync(CancellationToken ct = default);
    Task<int> CountActiveDriversAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<List<(string Date, int Count)>> GetDailyOrdersAsync(int days, CancellationToken ct = default);
    Task<List<(string Month, decimal Revenue)>> GetMonthlyRevenueAsync(int months, CancellationToken ct = default);
    Task<List<(Guid DriverId, string Name, int Count)>> GetTopDriversAsync(int top, CancellationToken ct = default);
    Task<List<(string Method, int Count, decimal Total)>> GetPaymentStatsAsync(CancellationToken ct = default);
    Task<Dictionary<string, int>> GetOrderStatusCountsAsync(CancellationToken ct = default);
}

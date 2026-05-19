using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Orders;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;

namespace DeliveryHub.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDriverRepository _driverRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IDriverRepository driverRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _driverRepository = driverRepository;
    }

    public async Task<PagedResult<OrderDto>> GetAllAsync(QueryParams query, string? status, Guid? customerId, Guid? driverId, CancellationToken ct = default)
    {
        var result = await _orderRepository.GetPagedAsync(query, status, customerId, driverId, ct);
        return new PagedResult<OrderDto>
        {
            Items = result.Items.Select(Map).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _orderRepository.GetWithDetailsAsync(id, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, ct)
            ?? throw new KeyNotFoundException("Customer not found.");

        var entity = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            PickupLocation = dto.PickupLocation,
            DeliveryLocation = dto.DeliveryLocation,
            PickupLatitude = dto.PickupLatitude,
            PickupLongitude = dto.PickupLongitude,
            DeliveryLatitude = dto.DeliveryLatitude,
            DeliveryLongitude = dto.DeliveryLongitude,
            DistanceKm = dto.DistanceKm,
            DeliveryFee = dto.DeliveryFee,
            OrderStatus = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await _orderRepository.AddAsync(entity, ct);
        await _orderRepository.SaveChangesAsync(ct);
        entity.Customer = customer;
        return Map(entity);
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderDto dto, CancellationToken ct = default)
    {
        var entity = await _orderRepository.GetWithDetailsAsync(id, ct);
        if (entity == null) return null;
        entity.PickupLocation = dto.PickupLocation;
        entity.DeliveryLocation = dto.DeliveryLocation;
        entity.PickupLatitude = dto.PickupLatitude;
        entity.PickupLongitude = dto.PickupLongitude;
        entity.DeliveryLatitude = dto.DeliveryLatitude;
        entity.DeliveryLongitude = dto.DeliveryLongitude;
        entity.DistanceKm = dto.DistanceKm;
        entity.DeliveryFee = dto.DeliveryFee;
        if (Enum.TryParse<OrderStatus>(dto.OrderStatus, true, out var status))
        {
            entity.OrderStatus = status;
            if (status == OrderStatus.Delivered)
                entity.DeliveredAt = DateTime.UtcNow;
        }
        _orderRepository.Update(entity);
        await _orderRepository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<OrderDto?> AssignDriverAsync(Guid id, AssignDriverDto dto, CancellationToken ct = default)
    {
        var entity = await _orderRepository.GetWithDetailsAsync(id, ct);
        var driver = await _driverRepository.GetByIdAsync(dto.DriverId, ct);
        if (entity == null || driver == null) return null;

        entity.DriverId = dto.DriverId;
        entity.Driver = driver;
        entity.OrderStatus = OrderStatus.Assigned;
        driver.Status = DriverStatus.Busy;
        _driverRepository.Update(driver);
        _orderRepository.Update(entity);
        await _orderRepository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<OrderDto?> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        var entity = await _orderRepository.GetWithDetailsAsync(id, ct);
        if (entity == null || !Enum.TryParse<OrderStatus>(dto.OrderStatus, true, out var status))
            return null;

        entity.OrderStatus = status;
        if (status == OrderStatus.Delivered)
        {
            entity.DeliveredAt = DateTime.UtcNow;
            if (entity.Driver != null)
            {
                entity.Driver.Status = DriverStatus.Available;
                _driverRepository.Update(entity.Driver);
            }
        }
        _orderRepository.Update(entity);
        await _orderRepository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _orderRepository.GetByIdAsync(id, ct);
        if (entity == null) return false;
        _orderRepository.Remove(entity);
        await _orderRepository.SaveChangesAsync(ct);
        return true;
    }

    private static OrderDto Map(Order o) =>
        new(o.Id, o.CustomerId, o.Customer?.FullName ?? "", o.DriverId, o.Driver?.FullName,
            o.PickupLocation, o.DeliveryLocation,
            o.PickupLatitude, o.PickupLongitude, o.DeliveryLatitude, o.DeliveryLongitude, o.DistanceKm,
            o.OrderStatus.ToString(), o.DeliveryFee,
            o.CreatedAt, o.DeliveredAt);
}

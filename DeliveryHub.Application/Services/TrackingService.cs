using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Tracking;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;

namespace DeliveryHub.Application.Services;

public class TrackingService : ITrackingService
{
    private readonly ITrackingRepository _repository;
    private readonly IOrderRepository _orderRepository;

    public TrackingService(ITrackingRepository repository, IOrderRepository orderRepository)
    {
        _repository = repository;
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<TrackingDto>> GetAllAsync(QueryParams query, Guid? orderId, CancellationToken ct = default)
    {
        var result = await _repository.GetPagedAsync(query, orderId, ct);
        return new PagedResult<TrackingDto>
        {
            Items = result.Items.Select(Map).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<TrackingDto?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var entity = await _repository.GetLatestByOrderIdAsync(orderId, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<List<TrackingDto>> GetHistoryByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var items = await _repository.GetHistoryByOrderIdAsync(orderId, ct);
        return items.Select(Map).ToList();
    }

    public async Task<TrackingDto> CreateAsync(CreateTrackingDto dto, CancellationToken ct = default)
    {
        _ = await _orderRepository.GetByIdAsync(dto.OrderId, ct)
            ?? throw new KeyNotFoundException("Order not found.");

        var statusKey = dto.TrackingStatus.Replace(" ", "", StringComparison.Ordinal);
        if (!Enum.TryParse<TrackingStatus>(statusKey, true, out var status))
            throw new ArgumentException("Invalid tracking status.");

        var entity = new Tracking
        {
            Id = Guid.NewGuid(),
            OrderId = dto.OrderId,
            CurrentLocation = dto.CurrentLocation,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            TrackingStatus = status,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<TrackingDto?> UpdateAsync(Guid id, UpdateTrackingDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null) return null;

        entity.CurrentLocation = dto.CurrentLocation;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        if (Enum.TryParse<TrackingStatus>(dto.TrackingStatus.Replace(" ", ""), true, out var status))
            entity.TrackingStatus = status;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        await _repository.SaveChangesAsync(ct);
        return Map(entity);
    }

    private static TrackingDto Map(Tracking t) =>
        new(t.Id, t.OrderId, t.CurrentLocation, t.Latitude, t.Longitude, t.TrackingStatus.ToString(), t.UpdatedAt);
}

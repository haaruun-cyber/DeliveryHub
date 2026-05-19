using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Drivers;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;

namespace DeliveryHub.Application.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _repository;

    public DriverService(IDriverRepository repository) => _repository = repository;

    public async Task<PagedResult<DriverDto>> GetAllAsync(QueryParams query, string? status, CancellationToken ct = default)
    {
        var result = await _repository.GetPagedAsync(query, status, ct);
        return new PagedResult<DriverDto>
        {
            Items = result.Items.Select(Map).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<DriverDto> CreateAsync(CreateDriverDto dto, CancellationToken ct = default)
    {
        var status = Enum.TryParse<DriverStatus>(dto.Status, true, out var s) ? s : DriverStatus.Available;
        var entity = new Driver
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Phone = dto.Phone,
            VehicleType = dto.VehicleType,
            LicenseNumber = dto.LicenseNumber,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<DriverDto?> UpdateAsync(Guid id, UpdateDriverDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null) return null;
        entity.FullName = dto.FullName;
        entity.Phone = dto.Phone;
        entity.VehicleType = dto.VehicleType;
        entity.LicenseNumber = dto.LicenseNumber;
        if (Enum.TryParse<DriverStatus>(dto.Status, true, out var status))
            entity.Status = status;
        _repository.Update(entity);
        await _repository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null) return false;
        _repository.Remove(entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    private static DriverDto Map(Driver d) =>
        new(d.Id, d.FullName, d.Phone, d.VehicleType, d.LicenseNumber, d.Status.ToString(), d.CreatedAt);
}

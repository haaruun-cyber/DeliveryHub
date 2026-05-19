using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Customers;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;

namespace DeliveryHub.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository) => _repository = repository;

    public async Task<PagedResult<CustomerDto>> GetAllAsync(QueryParams query, CancellationToken ct = default)
    {
        var result = await _repository.GetPagedAsync(query, ct);
        return new PagedResult<CustomerDto>
        {
            Items = result.Items.Select(Map).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Phone = dto.Phone,
            Address = dto.Address,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null) return null;
        entity.FullName = dto.FullName;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.Email = dto.Email;
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

    private static CustomerDto Map(Customer c) =>
        new(c.Id, c.FullName, c.Phone, c.Address, c.Email, c.CreatedAt);
}

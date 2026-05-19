using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Payments;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;

namespace DeliveryHub.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IOrderRepository _orderRepository;

    public PaymentService(IPaymentRepository repository, IOrderRepository orderRepository)
    {
        _repository = repository;
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<PaymentDto>> GetAllAsync(QueryParams query, string? status, CancellationToken ct = default)
    {
        var result = await _repository.GetPagedAsync(query, status, ct);
        return new PagedResult<PaymentDto>
        {
            Items = result.Items.Select(Map).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto, CancellationToken ct = default)
    {
        _ = await _orderRepository.GetByIdAsync(dto.OrderId, ct)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), true, out var method))
            throw new ArgumentException("Invalid payment method.");

        var entity = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            PaymentMethod = method,
            PaymentStatus = PaymentStatus.Pending
        };
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PaymentDto?> UpdateAsync(Guid id, UpdatePaymentDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null) return null;

        if (Enum.TryParse<PaymentStatus>(dto.PaymentStatus, true, out var status))
        {
            entity.PaymentStatus = status;
            if (status == PaymentStatus.Paid)
                entity.PaidAt = DateTime.UtcNow;
        }
        if (!string.IsNullOrEmpty(dto.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), true, out var method))
            entity.PaymentMethod = method;

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

    private static PaymentDto Map(Payment p) =>
        new(p.Id, p.OrderId, p.Amount, p.PaymentMethod.ToString(), p.PaymentStatus.ToString(), p.PaidAt);
}

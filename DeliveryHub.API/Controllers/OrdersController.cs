using System.Security.Claims;
using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Orders;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.API.Helpers;
using DeliveryHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IValidator<CreateOrderDto> _createValidator;

    public OrdersController(
        IOrderService service,
        ICustomerRepository customerRepository,
        IDriverRepository driverRepository,
        IValidator<CreateOrderDto> createValidator)
    {
        _service = service;
        _customerRepository = customerRepository;
        _driverRepository = driverRepository;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetAll(
        [FromQuery] QueryParams query,
        [FromQuery] string? status,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? driverId,
        CancellationToken ct)
    {
        var (scopedCustomerId, scopedDriverId) = await OrderAccessHelper.ResolveScopeAsync(
            User, _customerRepository, _driverRepository, customerId, driverId, ct);

        var result = await _service.GetAllAsync(query, status, scopedCustomerId, scopedDriverId, ct);
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id, CancellationToken ct)
    {
        var (scopedCustomerId, scopedDriverId) = await OrderAccessHelper.ResolveScopeAsync(
            User, _customerRepository, _driverRepository, null, null, ct);

        var result = await _service.GetByIdAsync(id, ct);
        if (result == null) return NotFound(ApiResponse<OrderDto>.Fail("Order not found."));

        if (User.IsInRole(UserRole.Admin.ToString()))
            return Ok(ApiResponse<OrderDto>.Ok(result));

        if (scopedCustomerId.HasValue && result.CustomerId != scopedCustomerId)
            return Forbid();
        if (scopedDriverId.HasValue && result.DriverId != scopedDriverId)
            return Forbid();

        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);

        if (User.IsInRole(UserRole.Customer.ToString()))
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var customer = await _customerRepository.GetByUserIdAsync(userId, ct)
                ?? throw new InvalidOperationException("Customer profile not found.");
            dto = dto with { CustomerId = customer.Id };
        }

        var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<OrderDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Update(Guid id, [FromBody] UpdateOrderDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result == null ? NotFound(ApiResponse<OrderDto>.Fail("Order not found.")) : Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpPatch("{id:guid}/assign-driver")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> AssignDriver(Guid id, [FromBody] AssignDriverDto dto, CancellationToken ct)
    {
        var result = await _service.AssignDriverAsync(id, dto, ct);
        return result == null ? NotFound(ApiResponse<OrderDto>.Fail("Order or driver not found.")) : Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
    {
        if (User.IsInRole(UserRole.Driver.ToString()))
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var driver = await _driverRepository.GetByUserIdAsync(userId, ct);
            var order = await _service.GetByIdAsync(id, ct);
            if (order == null) return NotFound(ApiResponse<OrderDto>.Fail("Order not found."));
            if (order.DriverId != driver?.Id)
                return Forbid();
        }

        var result = await _service.UpdateStatusAsync(id, dto, ct);
        return result == null ? NotFound(ApiResponse<OrderDto>.Fail("Order not found.")) : Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? Ok(ApiResponse<object>.Ok(new { }, "Order deleted.")) : NotFound(ApiResponse<object>.Fail("Order not found."));
    }
}

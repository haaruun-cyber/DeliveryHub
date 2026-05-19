using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Tracking;
using DeliveryHub.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _service;
    private readonly IValidator<CreateTrackingDto> _createValidator;

    public TrackingController(ITrackingService service, IValidator<CreateTrackingDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TrackingDto>>>> GetAll(
        [FromQuery] QueryParams query, [FromQuery] Guid? orderId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, orderId, ct);
        return Ok(ApiResponse<PagedResult<TrackingDto>>.Ok(result));
    }

    [HttpGet("order/{orderId:guid}/latest")]
    public async Task<ActionResult<ApiResponse<TrackingDto>>> GetLatest(Guid orderId, CancellationToken ct)
    {
        var result = await _service.GetLatestByOrderIdAsync(orderId, ct);
        return Ok(ApiResponse<TrackingDto?>.Ok(result));
    }

    [HttpGet("order/{orderId:guid}/history")]
    public async Task<ActionResult<ApiResponse<List<TrackingDto>>>> GetHistory(Guid orderId, CancellationToken ct)
    {
        var result = await _service.GetHistoryByOrderIdAsync(orderId, ct);
        return Ok(ApiResponse<List<TrackingDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<ApiResponse<TrackingDto>>> Create([FromBody] CreateTrackingDto dto, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<TrackingDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<ApiResponse<TrackingDto>>> Update(Guid id, [FromBody] UpdateTrackingDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result == null ? NotFound(ApiResponse<TrackingDto>.Fail("Tracking not found.")) : Ok(ApiResponse<TrackingDto>.Ok(result));
    }
}

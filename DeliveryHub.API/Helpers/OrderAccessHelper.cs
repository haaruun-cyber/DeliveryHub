using System.Security.Claims;
using DeliveryHub.Application.DTOs.Orders;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;

namespace DeliveryHub.API.Helpers;

public static class OrderAccessHelper
{
    public static async Task<(Guid? CustomerId, Guid? DriverId)> ResolveScopeAsync(
        ClaimsPrincipal user,
        ICustomerRepository customerRepo,
        IDriverRepository driverRepo,
        Guid? requestedCustomerId,
        Guid? requestedDriverId,
        CancellationToken ct)
    {
        var role = user.FindFirstValue(ClaimTypes.Role);
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (role == UserRole.Admin.ToString())
            return (requestedCustomerId, requestedDriverId);

        if (role == UserRole.Customer.ToString())
        {
            var customer = await customerRepo.GetByUserIdAsync(userId, ct);
            return (customer?.Id, null);
        }

        if (role == UserRole.Driver.ToString())
        {
            var driver = await driverRepo.GetByUserIdAsync(userId, ct);
            return (null, driver?.Id);
        }

        return (null, null);
    }

    public static void EnsureCanAccess(Order order, ClaimsPrincipal user, Guid? scopedCustomerId, Guid? scopedDriverId)
    {
        var role = user.FindFirstValue(ClaimTypes.Role);
        if (role == UserRole.Admin.ToString()) return;

        if (role == UserRole.Customer.ToString() && scopedCustomerId.HasValue && order.CustomerId == scopedCustomerId)
            return;

        if (role == UserRole.Driver.ToString() && scopedDriverId.HasValue && order.DriverId == scopedDriverId)
            return;

        throw new UnauthorizedAccessException("You do not have access to this order.");
    }
}

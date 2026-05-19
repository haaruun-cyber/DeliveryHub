using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeliveryHub.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        logger.LogInformation("Seeding database...");

        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            FullName = "System Admin",
            Email = "admin@deliveryhub.com",
            PasswordHash = hasher.Hash("Admin@123"),
            Role = UserRole.Admin,
            Phone = "+252611000001",
            CreatedAt = DateTime.UtcNow
        };

        var driverUserId = Guid.NewGuid();
        var driverUser = new User
        {
            Id = driverUserId,
            FullName = "Ahmed Hassan",
            Email = "driver@deliveryhub.com",
            PasswordHash = hasher.Hash("Driver@123"),
            Role = UserRole.Driver,
            Phone = "+252611000002",
            CreatedAt = DateTime.UtcNow
        };

        var customerUserId = Guid.NewGuid();
        var customerUser = new User
        {
            Id = customerUserId,
            FullName = "Fatima Ali",
            Email = "customer@deliveryhub.com",
            PasswordHash = hasher.Hash("Customer@123"),
            Role = UserRole.Customer,
            Phone = "+252611000003",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(admin, driverUser, customerUser);

        var customer1Id = Guid.NewGuid();
        var customer1 = new Customer
        {
            Id = customer1Id,
            FullName = "Fatima Ali",
            Phone = "+252611000003",
            Address = "Hodan District, Mogadishu",
            Email = "customer@deliveryhub.com",
            UserId = customerUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var customer2 = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Omar Yusuf",
            Phone = "+252611000004",
            Address = "Wadajir District, Mogadishu",
            Email = "omar@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        context.Customers.AddRange(customer1, customer2);

        var driver1Id = Guid.NewGuid();
        var driver1 = new Driver
        {
            Id = driver1Id,
            FullName = "Ahmed Hassan",
            Phone = "+252611000002",
            VehicleType = "Motorcycle",
            LicenseNumber = "DL-2024-001",
            Status = DriverStatus.Available,
            UserId = driverUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        };

        var driver2 = new Driver
        {
            Id = Guid.NewGuid(),
            FullName = "Hassan Mohamed",
            Phone = "+252611000005",
            VehicleType = "Van",
            LicenseNumber = "DL-2024-002",
            Status = DriverStatus.Busy,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        context.Drivers.AddRange(driver1, driver2);

        var order1Id = Guid.NewGuid();
        var order1 = new Order
        {
            Id = order1Id,
            CustomerId = customer1Id,
            DriverId = driver1Id,
            PickupLocation = "Bakaaraha Market, Mogadishu",
            DeliveryLocation = "Hodan District, Mogadishu",
            OrderStatus = OrderStatus.Delivered,
            DeliveryFee = 15.00m,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            DeliveredAt = DateTime.UtcNow.AddDays(-4)
        };

        var order2Id = Guid.NewGuid();
        var order2 = new Order
        {
            Id = order2Id,
            CustomerId = customer2.Id,
            DriverId = driver1Id,
            PickupLocation = "KM4 Junction, Mogadishu",
            DeliveryLocation = "Wadajir District, Mogadishu",
            OrderStatus = OrderStatus.InTransit,
            DeliveryFee = 20.00m,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var order3 = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1Id,
            PickupLocation = "Airport Road, Mogadishu",
            DeliveryLocation = "Lido Beach Area",
            OrderStatus = OrderStatus.Pending,
            DeliveryFee = 25.00m,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.AddRange(order1, order2, order3);

        context.Payments.AddRange(
            new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order1Id,
                Amount = 15.00m,
                PaymentMethod = PaymentMethod.EvcPlus,
                PaymentStatus = PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow.AddDays(-4)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order2Id,
                Amount = 20.00m,
                PaymentMethod = PaymentMethod.Cash,
                PaymentStatus = PaymentStatus.Pending
            }
        );

        context.Trackings.AddRange(
            new Tracking
            {
                Id = Guid.NewGuid(),
                OrderId = order2Id,
                CurrentLocation = "KM4 Junction",
                Latitude = 2.0469,
                Longitude = 45.3182,
                TrackingStatus = TrackingStatus.InTransit,
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Tracking
            {
                Id = Guid.NewGuid(),
                OrderId = order1Id,
                CurrentLocation = "Hodan District",
                Latitude = 2.0371,
                Longitude = 45.3438,
                TrackingStatus = TrackingStatus.Delivered,
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            }
        );

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded successfully.");
    }
}

using System.Security.Cryptography;
using DeliveryHub.Application.DTOs.Auth;
using DeliveryHub.Application.Interfaces;
using DeliveryHub.Domain.Entities;
using DeliveryHub.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DeliveryHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ICustomerRepository customerRepository,
        IDriverRepository driverRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _driverRepository = driverRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (await _userRepository.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("Email is already registered.");

        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            throw new ArgumentException("Invalid role.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = role,
            Phone = dto.Phone,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        if (role == UserRole.Customer)
        {
            await _customerRepository.AddAsync(new Customer
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Phone = dto.Phone ?? "",
                Address = "",
                Email = dto.Email,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            }, ct);
            await _customerRepository.SaveChangesAsync(ct);
        }
        else if (role == UserRole.Driver)
        {
            await _driverRepository.AddAsync(new Driver
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Phone = dto.Phone ?? "",
                VehicleType = "Motorcycle",
                LicenseNumber = "PENDING",
                Status = DriverStatus.Available,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            }, ct);
            await _driverRepository.SaveChangesAsync(ct);
        }

        _logger.LogInformation("User registered: {Email}", dto.Email);
        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");
        var (customerId, driverId) = await GetProfileIdsAsync(user, ct);
        return new UserProfileDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.Phone, user.ProfileImageUrl, customerId, driverId);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");
        user.FullName = dto.FullName;
        user.Phone = dto.Phone;
        _userRepository.Update(user);

        if (user.Role == UserRole.Customer)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId, ct);
            if (customer != null)
            {
                customer.FullName = dto.FullName;
                customer.Phone = dto.Phone ?? customer.Phone;
                _customerRepository.Update(customer);
            }
        }
        else if (user.Role == UserRole.Driver)
        {
            var driver = await _driverRepository.GetByUserIdAsync(userId, ct);
            if (driver != null)
            {
                driver.FullName = dto.FullName;
                driver.Phone = dto.Phone ?? driver.Phone;
                _driverRepository.Update(driver);
            }
        }

        await _userRepository.SaveChangesAsync(ct);
        return await GetMeAsync(userId, ct);
    }

    public async Task<UserProfileDto> UpdateProfileImageAsync(Guid userId, UpdateProfileImageDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.ProfileImageUrl = dto.ProfileImageUrl;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
        return await GetMeAsync(userId, ct);
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user, CancellationToken ct)
    {
        var (customerId, driverId) = await GetProfileIdsAsync(user, ct);
        return _tokenService.GenerateToken(user.Id, user.FullName, user.Email, user.Role.ToString(), user.ProfileImageUrl, customerId, driverId);
    }

    private async Task<(Guid? CustomerId, Guid? DriverId)> GetProfileIdsAsync(User user, CancellationToken ct)
    {
        Guid? customerId = null, driverId = null;
        if (user.Role == UserRole.Customer)
        {
            var c = await _customerRepository.GetByUserIdAsync(user.Id, ct);
            customerId = c?.Id;
        }
        else if (user.Role == UserRole.Driver)
        {
            var d = await _driverRepository.GetByUserIdAsync(user.Id, ct);
            driverId = d?.Id;
        }
        return (customerId, driverId);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task<string?> RequestPasswordResetAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLowerInvariant(), ct);
        if (user == null) return null;

        user.PasswordResetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
        _logger.LogInformation("Password reset requested for {Email}. Token: {Token}", dto.Email, user.PasswordResetToken);
        return user.PasswordResetToken;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.PasswordResetToken != dto.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired reset token.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
    }
}

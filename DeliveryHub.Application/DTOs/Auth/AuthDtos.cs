namespace DeliveryHub.Application.DTOs.Auth;

public record RegisterDto(string FullName, string Email, string Password, string Role, string? Phone);
public record LoginDto(string Email, string Password);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
public record ResetPasswordRequestDto(string Email);
public record ResetPasswordDto(string Email, string Token, string NewPassword);

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string? ProfileImageUrl,
    Guid? CustomerId = null,
    Guid? DriverId = null
);

public record UserProfileDto(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string? Phone,
    string? ProfileImageUrl,
    Guid? CustomerId,
    Guid? DriverId
);

public record UpdateProfileDto(string FullName, string? Phone);
public record UpdateProfileImageDto(string ProfileImageUrl);

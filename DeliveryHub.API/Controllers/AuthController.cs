using System.Security.Claims;
using DeliveryHub.API.Services;
using DeliveryHub.Application.Common;
using DeliveryHub.Application.DTOs.Auth;
using DeliveryHub.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        ICloudinaryService cloudinaryService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ChangePasswordDto> changePasswordValidator,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _cloudinaryService = cloudinaryService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _environment = environment;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        await _registerValidator.ValidateAndThrowAsync(dto, ct);
        var result = await _authService.RegisterAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        await _loginValidator.ValidateAndThrowAsync(dto, ct);
        var result = await _authService.LoginAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(dto, ct);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.ChangePasswordAsync(userId, dto, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Password changed successfully."));
    }

    [HttpPost("reset-password-request")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ResetPasswordRequest([FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
    {
        var token = await _authService.RequestPasswordResetAsync(dto, ct);
        object data = _environment.IsDevelopment() && token != null
            ? new { resetToken = token }
            : new { };
        return Ok(ApiResponse<object>.Ok(data, "If the email exists, a reset link has been sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(dto, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Password reset successful."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetMe(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _authService.GetMeAsync(userId, ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateMe([FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _authService.UpdateProfileAsync(userId, dto, ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Profile updated."));
    }

    [HttpPost("me/avatar")]
    [Authorize]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UploadAvatar([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<UserProfileDto>.Fail("Please select an image."));

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<UserProfileDto>.Fail("Only image files are allowed."));

        if (!_cloudinaryService.IsConfigured)
            return BadRequest(ApiResponse<UserProfileDto>.Fail(
                "Cloudinary is not configured. Add your Cloud Name, API Key, and API Secret to appsettings.Development.json."));

        var imageUrl = await _cloudinaryService.UploadProfileImageAsync(file, ct);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _authService.UpdateProfileImageAsync(userId, new UpdateProfileImageDto(imageUrl), ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Profile image updated."));
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResponse<object>> Logout() =>
        Ok(ApiResponse<object>.Ok(new { }, "Logged out successfully. Remove token on client."));
}

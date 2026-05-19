namespace DeliveryHub.API.Services;

public interface ICloudinaryService
{
    bool IsConfigured { get; }
    Task<string> UploadProfileImageAsync(IFormFile file, CancellationToken ct = default);
}

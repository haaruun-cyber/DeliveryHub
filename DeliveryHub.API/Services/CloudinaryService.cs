using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DeliveryHub.API.Services;

public class CloudinaryService : ICloudinaryService
{
    private const string ProfileFolder = "deliveryhub/profile-images";
    private readonly Cloudinary? _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
            ?? configuration["Cloudinary:CloudName"];
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
            ?? configuration["Cloudinary:ApiKey"];
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            ?? configuration["Cloudinary:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(cloudName)
            && !string.IsNullOrWhiteSpace(apiKey)
            && !string.IsNullOrWhiteSpace(apiSecret))
        {
            _cloudinary = new Cloudinary(new Account(cloudName.Trim(), apiKey.Trim(), apiSecret.Trim()));
        }
    }

    public bool IsConfigured => _cloudinary != null;

    public async Task<string> UploadProfileImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (_cloudinary == null)
            throw new InvalidOperationException(
                "Cloudinary is not configured. Set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret in appsettings.Development.json or use environment variables.");

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = ProfileFolder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false,
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");

        if (string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
            throw new InvalidOperationException("Cloudinary did not return an image URL.");

        return result.SecureUrl.ToString();
    }
}

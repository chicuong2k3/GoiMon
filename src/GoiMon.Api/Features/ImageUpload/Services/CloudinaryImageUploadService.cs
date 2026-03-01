using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GoiMon.Api.Features.ImageUpload.Services;

/// <summary>
/// Cloudinary-based image upload service.
/// </summary>
public class CloudinaryImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryImageUploadService> _logger;

    public CloudinaryImageUploadService(Cloudinary cloudinary, ILogger<CloudinaryImageUploadService> logger)
    {
        _cloudinary = cloudinary;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation("Image uploaded successfully: {Url}", uploadResult.SecureUrl);
        return uploadResult.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        // Extract public_id from Cloudinary URL
        // Example: https://res.cloudinary.com/{cloud}/image/upload/v123456/{public_id}.jpg
        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');
        var publicIdWithExtension = segments[^1]; // last segment
        var publicId = System.IO.Path.GetFileNameWithoutExtension(publicIdWithExtension);

        // Include folder if present
        if (segments.Length > 2)
        {
            var folder = segments[^2];
            publicId = $"{folder}/{publicId}";
        }

        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Result != "ok")
        {
            _logger.LogWarning("Failed to delete image {PublicId}: {Result}", publicId, result.Result);
        }
        else
        {
            _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);
        }
    }

    public string GetThumbnailUrl(string imageUrl, int width = 300, int height = 300)
    {
        // Cloudinary transformation URL pattern
        // Replace /upload/ with /upload/w_{width},h_{height},c_fill/
        if (!imageUrl.Contains("/upload/"))
        {
            _logger.LogWarning("Invalid Cloudinary URL: {Url}", imageUrl);
            return imageUrl;
        }

        return imageUrl.Replace("/upload/", $"/upload/w_{width},h_{height},c_fill/");
    }
}

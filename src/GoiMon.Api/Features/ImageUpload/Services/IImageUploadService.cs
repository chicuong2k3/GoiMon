namespace GoiMon.Api.Features.ImageUpload.Services;

/// <summary>
/// Service for uploading images to cloud storage (Cloudinary).
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Upload an image file and return the public URL.
    /// </summary>
    /// <param name="fileStream">Image file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Cloudinary folder (e.g., "products", "combos")</param>
    /// <returns>Public URL of the uploaded image</returns>
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Delete an image from cloud storage by public URL.
    /// </summary>
    /// <param name="imageUrl">The full public URL</param>
    Task DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Generate a thumbnail URL with specified dimensions.
    /// </summary>
    /// <param name="imageUrl">Original image URL</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <returns>Thumbnail URL with transformations</returns>
    string GetThumbnailUrl(string imageUrl, int width = 300, int height = 300);
}

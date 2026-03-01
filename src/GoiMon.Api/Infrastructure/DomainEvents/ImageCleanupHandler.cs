using GoiMon.Api.Domain.Events;
using GoiMon.Api.Features.ImageUpload.Services;

namespace GoiMon.Api.Infrastructure.DomainEvents;

/// <summary>
/// Handles image cleanup when images are replaced or entities are deleted.
/// Runs via outbox pattern (background), so Cloudinary failures don't affect the main request.
/// </summary>
public class ImageCleanupHandler :
    IDomainEventHandler<ProductImageUpdatedEvent>,
    IDomainEventHandler<ProductDeletedEvent>,
    IDomainEventHandler<ComboImageUpdatedEvent>,
    IDomainEventHandler<ComboDeletedEvent>
{
    private readonly IImageUploadService _imageUpload;
    private readonly ILogger<ImageCleanupHandler> _logger;

    public ImageCleanupHandler(IImageUploadService imageUpload, ILogger<ImageCleanupHandler> logger)
    {
        _imageUpload = imageUpload;
        _logger = logger;
    }

    public async Task HandleAsync(ProductImageUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        await DeleteIfReplacedAsync(@event.OldImageUrl, @event.NewImageUrl);
    }

    public async Task HandleAsync(ProductDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        await DeleteIfExistsAsync(@event.ImageUrl);
    }

    public async Task HandleAsync(ComboImageUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        await DeleteIfReplacedAsync(@event.OldImageUrl, @event.NewImageUrl);
    }

    public async Task HandleAsync(ComboDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        await DeleteIfExistsAsync(@event.ImageUrl);
    }

    private async Task DeleteIfReplacedAsync(string? oldUrl, string? newUrl)
    {
        if (!string.IsNullOrEmpty(oldUrl) && oldUrl != newUrl)
            await DeleteIfExistsAsync(oldUrl);
    }

    private async Task DeleteIfExistsAsync(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;
        try
        {
            await _imageUpload.DeleteImageAsync(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image {ImageUrl}", imageUrl);
        }
    }
}

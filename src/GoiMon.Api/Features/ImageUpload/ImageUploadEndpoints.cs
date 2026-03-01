using GoiMon.Api.Features.ImageUpload.Services;

namespace GoiMon.Api.Features.ImageUpload;

public static class ImageUploadEndpoints
{
    public static void MapImageUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/api/upload", async (HttpRequest request, IImageUploadService imageUpload) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Expected multipart form data.");

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
                return Results.BadRequest("No file provided.");

            var folder = request.Query["folder"].FirstOrDefault() ?? "uploads";

            if (!file.ContentType.StartsWith("image/"))
                return Results.BadRequest("Only image files are allowed.");

            if (file.Length > 10 * 1024 * 1024)
                return Results.BadRequest("File size exceeds 10 MB limit.");

            await using var stream = file.OpenReadStream();
            var url = await imageUpload.UploadImageAsync(stream, file.FileName, folder);

            return Results.Ok(new { url });
        })
        .DisableAntiforgery()
        .RequireCors("AllowNitro");
    }
}

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace GoiMon.Staff.Features.ImageUpload.Services;

public interface IImageUploadService
{
    Task<string?> UploadAsync(IBrowserFile file, string folder, CancellationToken ct = default);
}

public class ImageUploadService(HttpClient http, IWebAssemblyHostEnvironment env) : IImageUploadService
{
    private string ApiBase => env.IsDevelopment() ? "http://localhost:5000" : string.Empty;

    public async Task<string?> UploadAsync(IBrowserFile file, string folder, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024, cancellationToken: ct);
        await using var _ = stream;
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.Name);

        var response = await http.PostAsync($"{ApiBase}/api/upload?folder={folder}", content, ct);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<UploadResult>(cancellationToken: ct);
        return result?.Url;
    }

    private record UploadResult(string Url);
}

using Backtrack.Core.Application.Interfaces.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Backtrack.Core.Infrastructure.Helpers;

public sealed class HttpImageFetcher : IImageFetcher
{
    private readonly HttpClient _httpClient;

    public HttpImageFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FetchedImage?> FetchAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (bytes.Length == 0)
                return null;

            // Convert to JPEG regardless of original format (WebP, PNG, GIF, etc.)
            // Gemini embedding API only supports PNG and JPEG
            using var inputStream = new MemoryStream(bytes);
            using var image = await Image.LoadAsync(inputStream, cancellationToken);
            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 90 }, cancellationToken);

            return new FetchedImage(Convert.ToBase64String(outputStream.ToArray()), "image/jpeg");
        }
        catch
        {
            // Return null so the caller falls back to text-only embedding
            return null;
        }
    }
}

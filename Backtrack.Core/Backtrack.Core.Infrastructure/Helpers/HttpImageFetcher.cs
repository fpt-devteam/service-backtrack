using Backtrack.Core.Application.Interfaces.Helpers;
using ImageMagick;

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

            // Convert to JPEG regardless of original format (WebP, PNG, GIF, HEIC, etc.)
            // Gemini embedding API only supports PNG and JPEG
            using var image = new MagickImage(bytes);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 90;

            return new FetchedImage(Convert.ToBase64String(image.ToByteArray()), "image/jpeg");
        }
        catch
        {
            // Return null so the caller falls back to text-only embedding
            return null;
        }
    }
}

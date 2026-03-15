namespace Backtrack.Core.Application.Interfaces.Helpers;

public interface IImageFetcher
{
    /// <summary>
    /// Downloads an image from a URL and returns it as base64-encoded data with its MIME type.
    /// Returns null if the image cannot be fetched.
    /// </summary>
    Task<FetchedImage?> FetchAsync(string url, CancellationToken cancellationToken = default);
}

public sealed record FetchedImage(string Base64, string MimeType);

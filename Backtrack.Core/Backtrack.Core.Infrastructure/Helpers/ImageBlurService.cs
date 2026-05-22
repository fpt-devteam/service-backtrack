using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Storage;
using ImageMagick;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Helpers;

public sealed class ImageBlurService(
    IImageFetcher imageFetcher,
    IFirebaseStorageService storageService,
    ILogger<ImageBlurService> logger) : IImageBlurService
{
    private const int BlurRadius = 0;
    private const double BlurSigma = 8.0;
    private const int MaxWidth = 400;
    private const int MaxHeight = 300;

    public async Task<List<string>> GenerateAndUploadAsync(
        IEnumerable<string> imageUrls,
        string folderPrefix,
        CancellationToken ct = default)
    {
        var blurUrls = new List<string>();

        foreach (var url in imageUrls)
        {
            try
            {
                var fetched = await imageFetcher.FetchAsync(url, ct);
                if (fetched is null) continue;

                var originalBytes = Convert.FromBase64String(fetched.Base64);

                using var image = new MagickImage(originalBytes);
                image.Resize(new MagickGeometry(MaxWidth, MaxHeight) { IgnoreAspectRatio = false, Greater = true });
                image.GaussianBlur(BlurRadius, BlurSigma);
                image.Format = MagickFormat.Jpeg;
                image.Quality = 75;

                var blurBytes = image.ToByteArray();
                var fileName = $"{folderPrefix}/{Guid.NewGuid():N}.jpg";
                var uploadedUrl = await storageService.UploadAsync(blurBytes, fileName, "image/jpeg", ct);
                blurUrls.Add(uploadedUrl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to generate blur image for URL: {Url}", url);
            }
        }

        return blurUrls;
    }
}

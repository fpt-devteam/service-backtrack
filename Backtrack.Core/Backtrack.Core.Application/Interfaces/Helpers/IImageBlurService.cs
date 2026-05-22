namespace Backtrack.Core.Application.Interfaces.Helpers;

public interface IImageBlurService
{
    Task<List<string>> GenerateAndUploadAsync(IEnumerable<string> imageUrls, string folderPrefix, CancellationToken ct = default);
}

namespace Backtrack.Core.Application.Interfaces.Storage;

public interface IFirebaseStorageService
{
    Task<string> UploadAsync(byte[] data, string fileName, string contentType, CancellationToken ct = default);
}

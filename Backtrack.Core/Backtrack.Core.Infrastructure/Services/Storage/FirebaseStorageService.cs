using Backtrack.Core.Application.Interfaces.Storage;
using Backtrack.Core.Infrastructure.Configurations;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace Backtrack.Core.Infrastructure.Services.Storage;

public sealed class FirebaseStorageService : IFirebaseStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucket;

    public FirebaseStorageService(IOptions<FirebaseSettings> settings)
    {
        var jsonBytes = Convert.FromBase64String(settings.Value.ServiceAccountJsonBase64);
        using var ms = new MemoryStream(jsonBytes);
        var credential = GoogleCredential.FromStream(ms)
            .CreateScoped("https://www.googleapis.com/auth/devstorage.full_control");

        _storageClient = StorageClient.Create(credential);
        _bucket = settings.Value.StorageBucket
            ?? $"{settings.Value.ProjectId}.appspot.com";
    }

    public async Task<string> UploadAsync(byte[] data, string fileName, string contentType, CancellationToken ct = default)
    {
        using var stream = new MemoryStream(data);
        var obj = await _storageClient.UploadObjectAsync(
            _bucket,
            fileName,
            contentType,
            stream,
            new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead },
            cancellationToken: ct);

        return $"https://storage.googleapis.com/{_bucket}/{Uri.EscapeDataString(obj.Name)}";
    }
}

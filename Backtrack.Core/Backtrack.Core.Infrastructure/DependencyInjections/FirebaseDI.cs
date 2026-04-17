using Backtrack.Core.Infrastructure.Configurations;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class FirebaseDI
{
    public static void AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FirebaseSettings>(configuration.GetSection("FirebaseSettings"));
        services.Configure<SuperAdminSettings>(configuration.GetSection("SuperAdminSettings"));

        var jsonBase64 = configuration["FirebaseSettings:ServiceAccountJsonBase64"];
        var projectId  = configuration["FirebaseSettings:ProjectId"];

        if (string.IsNullOrWhiteSpace(jsonBase64))
            throw new InvalidOperationException(
                "Firebase credentials not found. Set FirebaseSettings:ServiceAccountJsonBase64.");

        if (FirebaseApp.DefaultInstance is null)
        {
            var jsonBytes = Convert.FromBase64String(jsonBase64);
            using var ms  = new MemoryStream(jsonBytes);

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromStream(ms),
                ProjectId  = projectId
            });
        }
    }
}

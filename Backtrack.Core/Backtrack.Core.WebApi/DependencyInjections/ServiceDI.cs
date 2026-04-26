using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Interfaces.Email;
using Backtrack.Core.Application.Interfaces.PushNotification;
using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Application.Usecases.Users.EnsureUserExist;
using Backtrack.Core.Infrastructure.Helpers;
using Backtrack.Core.Infrastructure.Messaging;
using Backtrack.Core.Infrastructure.Messaging.Consumers;
using Backtrack.Core.Infrastructure.Repositories;
using Backtrack.Core.Infrastructure.Services.Notifications;
using FluentValidation;
using MediatR;

namespace Backtrack.Core.WebApi.DependencyInjections;

public static class ServiceDI
{
    public static void AddServiceConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR - Register handlers from Application layer
        services.AddMediatR(typeof(EnsureUserExistCommand).Assembly);

        // Add validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // FluentValidation - Register validators from Application layer
        services.AddValidatorsFromAssemblyContaining<CreatePostCommandValidator>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IJoinInvitationRepository, JoinInvitationRepository>();
        services.AddScoped<IPostMatchRepository, PostMatchRepository>();
        services.AddScoped<IC2CReturnReportRepository, ReturnReportRepository>();
        services.AddScoped<IOrgReturnReportRepository, OrgReturnReportRepository>();
        services.AddScoped<IOrgReceiveReportRepository, OrgReceiveReportRepository>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(CrudRepositoryBase<,>));
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();

        // Messaging
        services.AddScoped<IEventPublisher, CapEventPublisher>();
        services.AddScoped<InvitationCreatedConsumer>();

        // Notification services
        services.AddScoped<IPushNotificationService, ExpoPushNotificationService>();
        services.AddHttpClient();
        services.AddScoped<IEmailService, ResendEmailService>();

        services.AddHttpClient("Resend", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var apiKey = config["Resend:ApiKey"]
                ?? throw new InvalidOperationException("Resend:ApiKey is required.");
            client.BaseAddress = new Uri("https://api.resend.com/");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        });

        // Helpers
        services.AddSingleton<IHasher, SHA256Hasher>();

        // Request logging middleware
        services.AddTransient<RequestLoggingMiddleware>();
    }
}

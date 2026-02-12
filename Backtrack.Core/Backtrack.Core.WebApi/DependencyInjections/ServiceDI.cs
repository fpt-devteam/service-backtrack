using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.Posts.Commands.CreatePost;
using Backtrack.Core.Application.Usecases.Users.Commands.EnsureUserExist;
using Backtrack.Core.Infrastructure.Helpers;
using Backtrack.Core.Infrastructure.Messaging;
using Backtrack.Core.Infrastructure.Repositories;
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
        services.AddScoped(typeof(IGenericRepository<,>), typeof(CrudRepositoryBase<,>));

        // Messaging
        services.AddScoped<IEventPublisher, CapEventPublisher>();

        // Helpers
        services.AddSingleton<IHasher, SHA256Hasher>();

        // Request logging middleware
        services.AddTransient<RequestLoggingMiddleware>();
    }
}

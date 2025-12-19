using Backtrack.Core.Application.Common.Behaviors;
using Backtrack.Core.Application.Common.Interfaces.Helpers;
using Backtrack.Core.Application.Common.Interfaces.Repositories;
using Backtrack.Core.Application.Posts;
using Backtrack.Core.Application.Posts.Commands.CreatePost;
using Backtrack.Core.Application.Posts.Queries.GetPosts;
using Backtrack.Core.Application.Users;
using Backtrack.Core.Application.Users.Commands.CreateUser;
using Backtrack.Core.Application.Users.Queries.GetMe;
using Backtrack.Core.Infrastructure.Helpers;
using Backtrack.Core.Infrastructure.Repositories;
using Backtrack.Core.Infrastructure.Repositories.Common;
using FluentValidation;
using MediatR;

namespace Backtrack.Core.WebApi.DependencyInjections;

public static class ServiceDI
{
    public static void AddServiceConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR - Register handlers from Application layer
        services.AddMediatR(typeof(CreateUserCommand).Assembly);

        // Add validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // FluentValidation - Register validators from Application layer
        services.AddValidatorsFromAssemblyContaining<CreatePostCommandValidator>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(CrudRepositoryBase<,>));

        // Helpers
        services.AddSingleton<IHasher, SHA256Hasher>();
    }
}

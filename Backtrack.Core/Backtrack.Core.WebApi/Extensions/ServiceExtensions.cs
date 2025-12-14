using Backtrack.Core.Application.Common.Interfaces;
using Backtrack.Core.Application.Users;
using Backtrack.Core.Application.Users.Commands.CreateUser;
using Backtrack.Core.Contract.Users.Requests;
using Backtrack.Core.Infrastructure.Repositories;
using Backtrack.Core.Infrastructure.Repositories.Common;
using FluentValidation;
using MediatR;

namespace Backtrack.Core.WebApi.Extensions;

public static class ServiceExtensions
{
    public static void AddServiceConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR - Register handlers from Application layer
        services.AddMediatR(typeof(CreateUserCommand).Assembly);

        // FluentValidation - Register validators from Contract layer
        services.AddValidatorsFromAssemblyContaining<CreateUserRequest>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(CrudRepositoryBase<,>));
    }
}

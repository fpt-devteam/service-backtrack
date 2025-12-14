using Backtrack.Core.Contract.Users.Requests;
using Backtrack.Core.Contract.Users.Responses;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(CreateUserRequest Request) : IRequest<UserResponse>;

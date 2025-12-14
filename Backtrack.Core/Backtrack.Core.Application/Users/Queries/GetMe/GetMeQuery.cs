using Backtrack.Core.Contract.Users.Responses;
using MediatR;

namespace Backtrack.Core.Application.Users.Queries.GetMe;

public sealed record GetMeQuery(string UserId) : IRequest<UserResponse>;

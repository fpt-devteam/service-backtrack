using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.Queries.GetMe;

public sealed record GetMeQuery(string UserId) : IRequest<UserResult>;

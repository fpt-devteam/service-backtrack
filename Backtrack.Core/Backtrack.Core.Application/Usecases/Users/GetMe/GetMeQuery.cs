using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.GetMe;

public sealed record GetMeQuery(string UserId) : IRequest<UserResult>;

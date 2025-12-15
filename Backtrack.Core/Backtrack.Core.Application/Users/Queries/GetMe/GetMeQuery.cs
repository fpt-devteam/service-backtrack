using Backtrack.Core.Application.Users.Common;
using MediatR;

namespace Backtrack.Core.Application.Users.Queries.GetMe;

public sealed record GetMeQuery(string UserId) : IRequest<UserResult>;

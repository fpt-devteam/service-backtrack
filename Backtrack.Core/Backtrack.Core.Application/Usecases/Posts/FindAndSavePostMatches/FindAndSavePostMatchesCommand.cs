using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.FindAndSavePostMatches;

public sealed record FindAndSavePostMatchesCommand(Guid PostId) : IRequest;

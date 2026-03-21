using MediatR;

namespace Backtrack.Core.Application.Usecases.PostMatchings.FindAndSavePostMatches;

public sealed record FindAndSavePostMatchesCommand(Guid PostId) : IRequest;

using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.AI;

public sealed record PostMatchContext
{
    public required ItemCategory Category { get; init; }
    public required string LostDescription { get; init; }
    public required string FoundDescription { get; init; }
}

public sealed record PostMatchAssessment
{
    public required bool IsMatch { get; set; }
    public required string Reasoning { get; set; }
    public required List<MatchEvidence> Evidence { get; init; }
}

public interface IPostMatchAssessor
{
    Task<PostMatchAssessment> AssessAsync(
        PostMatchContext context,
        CancellationToken cancellationToken = default);
}

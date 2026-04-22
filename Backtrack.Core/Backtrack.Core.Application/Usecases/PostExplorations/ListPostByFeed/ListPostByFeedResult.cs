namespace Backtrack.Core.Application.Usecases.PostExplorations.ListPostByFeed;

public sealed record ListPostByFeedResult
{
    public List<SearchPostResult> PersonalBelongings { get; init; } = [];
    public List<SearchPostResult> Cards { get; init; } = [];
    public List<SearchPostResult> Electronics { get; init; } = [];
    public List<SearchPostResult> Others { get; init; } = [];
}

using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.ReturnReport;

public sealed record C2CReturnReportResult
{
    public required Guid Id { get; init; }
    public required UserResult Finder { get; init; }
    public UserResult? Owner { get; init; }
    public PostResult? FinderPost { get; init; }
    public PostResult? OwnerPost { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed record OrgReturnReportResult
{
    public required Guid Id { get; init; }
    public required OrganizationResult Organization { get; init; }
    public required UserResult Staff { get; init; }
    public OwnerInfo? OwnerInfo { get; init; }
    public PostResult? Post { get; init; }
}

using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.ReturnReport;

public sealed record C2CReturnReportResult
{
    public required Guid Id { get; init; }
    public required UserResult Finder { get; init; }
    public required UserResult Owner { get; init; }
    public required PostResult FinderPost { get; init; }
    public required PostResult OwnerPost { get; init; }
    public required C2CReturnReportStatus Status { get; init; }
    public List<string>? EvidenceImageUrls { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed record OrgReturnReportResult
{
    public required Guid Id { get; init; }
    public required OrganizationResult Organization { get; init; }
    public required UserResult Staff { get; init; }
    public required OwnerInfo OwnerInfo { get; init; }
    public required PostResult Post { get; init; }
    public required List<string> EvidenceImageUrls { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
public static class C2CReturnReportResultMapper
{
    public static C2CReturnReportResult ToC2CReturnReportResult(this C2CReturnReport report) => new()
    {
        Id              = report.Id,
        Finder          = report.Finder.ToUserResult(),
        Owner           = report.Owner.ToUserResult(),
        FinderPost      = report.FinderPost.ToPostResult(),
        OwnerPost       = report.OwnerPost.ToPostResult(),
        Status          = report.Status,
        EvidenceImageUrls = report.EvidenceImageUrls,
        DeliveredAt     = report.DeliveredAt,
        ConfirmedAt     = report.ConfirmedAt,
        ExpiresAt       = report.ExpiresAt,
        CreatedAt       = report.CreatedAt,
    };
}

public static class OrgReturnReportResultMapper
{
    public static OrgReturnReportResult ToOrgReturnReportResult(this OrgReturnReport report) => new()
    {
        Id = report.Id,
        Organization = report.Organization.ToOrganizationResult(),
        Staff = report.Staff.ToUserResult(),
        OwnerInfo = report.OwnerInfo,
        Post = report.Post.ToPostResult(),
        EvidenceImageUrls = report.EvidenceImageUrls,
        CreatedAt = report.CreatedAt,
    };
}

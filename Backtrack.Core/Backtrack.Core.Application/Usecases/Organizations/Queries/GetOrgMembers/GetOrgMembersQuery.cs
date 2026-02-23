using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.GetOrgMembers;

public sealed record GetOrgMembersQuery : IRequest<PagedResult<MemberResult>>
{
    public Guid OrgId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

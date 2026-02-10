using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.GetOrgMembers;

public sealed class GetOrgMembersHandler : IRequestHandler<GetOrgMembersQuery, PagedResult<MemberResult>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetOrgMembersHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<PagedResult<MemberResult>> Handle(GetOrgMembersQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.UserId))
        {
            throw new InvalidOperationException("UserId is not provided when initializing GetOrgMembersQuery.");
        }
        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        var paged = PagedQuery.FromPage(query.Page, query.PageSize);

        var (items, totalCount) = await _membershipRepository.GetPagedByOrgAsync(
            query.OrgId, paged.Offset, paged.Limit, cancellationToken);

        var results = items.Select(m => new MemberResult
        {
            MembershipId = m.Id,
            UserId = m.UserId,
            DisplayName = m.User.DisplayName,
            Email = m.User.Email,
            AvatarUrl = m.User.AvatarUrl,
            Role = m.Role.ToString(),
            Status = m.Status.ToString(),
            JoinedAt = m.JoinedAt,
        }).ToList();

        return new PagedResult<MemberResult>(totalCount, results);
    }
}

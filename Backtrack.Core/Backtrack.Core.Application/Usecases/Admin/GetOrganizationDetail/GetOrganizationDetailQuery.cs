using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizationDetail;

public sealed record GetOrganizationDetailQuery : IRequest<AdminOrgDetailResult>
{
    public required string AdminUserId { get; init; }
    public required Guid OrgId { get; init; }
    public int BillingPage { get; init; } = 1;
    public int BillingPageSize { get; init; } = 20;
}

using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Application.Usecases.Subscriptions.GetSubscription;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizationDetail;

public sealed class GetOrganizationDetailHandler(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository,
    ISubscriptionRepository subscriptionRepository,
    IMembershipRepository membershipRepository,
    IPostRepository postRepository,
    IPaymentHistoryRepository paymentHistoryRepository) : IRequestHandler<GetOrganizationDetailQuery, AdminOrgDetailResult>
{
    public async Task<AdminOrgDetailResult> Handle(GetOrganizationDetailQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var org = await organizationRepository.GetByIdAsync(query.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        var (subscription, memberCount, totalPostCount, activePostCount, (payments, _)) =
        (
            await subscriptionRepository.GetActiveByOrganizationIdAsync(org.Id, cancellationToken),
            await membershipRepository.CountByOrgAsync(org.Id, cancellationToken),
            await postRepository.CountAsync(new PostFilters { OrganizationId = org.Id }, cancellationToken),
            await postRepository.CountAsync(new PostFilters { OrganizationId = org.Id, Status = PostStatus.Active }, cancellationToken),
            await paymentHistoryRepository.GetPagedByOrgIdAsync(org.Id, query.BillingPage, query.BillingPageSize, cancellationToken)
        );

        return new AdminOrgDetailResult
        {
            BasicInfo = new OrganizationResult
            {
                Id                           = org.Id,
                Name                         = org.Name,
                Slug                         = org.Slug,
                Location                     = org.Location,
                DisplayAddress               = org.DisplayAddress,
                ExternalPlaceId              = org.ExternalPlaceId,
                Phone                        = org.Phone,
                ContactEmail                 = org.ContactEmail,
                IndustryType                 = org.IndustryType,
                TaxIdentificationNumber      = org.TaxIdentificationNumber,
                LogoUrl                      = org.LogoUrl,
                CoverImageUrl                = org.CoverImageUrl,
                LocationNote                 = org.LocationNote,
                BusinessHours                = org.BusinessHours,
                RequiredFinderContractFields = org.RequiredFinderContractFields,
                RequiredOwnerContractFields  = org.RequiredOwnerContractFields,
                Status                       = org.Status.ToString(),
                CreatedAt                    = org.CreatedAt
            },
            Subscription = subscription is null ? null : GetSubscriptionHandler.MapToResult(subscription),
            UsageOverview = new OrgUsageOverview
            {
                MemberCount      = memberCount,
                TotalPostCount   = totalPostCount,
                ActivePostCount  = activePostCount
            },
            BillingHistory = payments.Select(p => new PaymentHistoryResult
            {
                Id                = p.Id,
                Amount            = p.Amount,
                Currency          = p.Currency,
                Status            = p.Status,
                PaymentDate       = p.PaymentDate,
                ProviderInvoiceId = p.ProviderInvoiceId,
                PlanName          = p.Subscription?.PlanSnapshot?.Name
            }).ToList()
        };
    }
}

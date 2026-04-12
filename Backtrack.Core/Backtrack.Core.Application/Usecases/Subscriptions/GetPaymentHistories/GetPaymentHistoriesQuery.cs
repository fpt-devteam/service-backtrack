using Backtrack.Core.Application.Usecases;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.GetPaymentHistories;

public sealed record GetPaymentHistoriesQuery : IRequest<PagedResult<PaymentHistoryResult>>
{
    public required SubscriberContext Subscriber { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

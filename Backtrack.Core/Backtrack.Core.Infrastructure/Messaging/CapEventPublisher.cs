using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Domain.Constants;
using DotNetCore.CAP;

namespace Backtrack.Core.Infrastructure.Messaging;

public sealed class CapEventPublisher : IEventPublisher
{
    private readonly ICapPublisher _capPublisher;

    public CapEventPublisher(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public async Task PublishUserEnsureExistAsync(UserEnsureExistIntegrationEvent @event)
    {
        await _capPublisher.PublishAsync(EventTopics.User.EnsureExist, @event);
    }

    public async Task PublishInvitationCreatedAsync(InvitationCreatedIntegrationEvent @event)
    {
        await _capPublisher.PublishAsync(EventTopics.Invitation.Created, @event);
    }


    public async Task PublishReturnReportConfirmedAsync(ReturnReportConfirmedIntegrationEvent @event)
    {
        await _capPublisher.PublishAsync(EventTopics.ReturnReportConfirmed, @event);
    }

    public async Task PublishReturnReportSyncAsync(ReturnReportSyncIntegrationEvent @event)
    {
        await _capPublisher.PublishAsync(EventTopics.ReturnReportSynced, @event);
    }

    public async Task PublishOrgEnsureExistAsync(OrgEnsureExistIntegrationEvent @event)
    {
        await _capPublisher.PublishAsync(EventTopics.Org.EnsureExist, @event);
    }
}

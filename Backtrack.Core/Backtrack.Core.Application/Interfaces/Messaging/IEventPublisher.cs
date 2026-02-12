using Backtrack.Core.Application.Events;

namespace Backtrack.Core.Application.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishUserEnsureExistAsync(UserEnsureExistIntegrationEvent @event);
    Task PublishInvitationCreatedAsync(InvitationCreatedIntegrationEvent @event);
}

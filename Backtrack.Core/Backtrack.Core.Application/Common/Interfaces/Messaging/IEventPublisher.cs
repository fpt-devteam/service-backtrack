using Backtrack.Core.Application.Events.Integration;

namespace Backtrack.Core.Application.Common.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishUserEnsureExistAsync(UserEnsureExistIntegrationEvent @event);
}

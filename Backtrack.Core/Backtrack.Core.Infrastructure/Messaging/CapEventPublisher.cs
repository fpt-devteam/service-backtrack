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
}

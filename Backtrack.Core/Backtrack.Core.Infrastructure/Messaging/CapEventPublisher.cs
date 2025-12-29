using Backtrack.Core.Application.Common.Interfaces.Messaging;
using Backtrack.Core.Application.Events.Integration;
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

    public async Task PublishUserUpsertedAsync(UserUpsertedIntegrationEvent @event)
    {
        await _capPublisher.PublishAsync(EventTopics.User.Upserted, @event);
    }
}

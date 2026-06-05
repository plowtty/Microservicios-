namespace Shared.EventBus.Interfaces;

using Shared.EventBus.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IntegrationEvent;
}

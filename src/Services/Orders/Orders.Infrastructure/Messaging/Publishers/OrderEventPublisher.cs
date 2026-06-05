namespace Orders.Infrastructure.Messaging.Publishers;

using MassTransit;
using Shared.EventBus.Events;
using Shared.EventBus.Interfaces;

public class OrderEventPublisher : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderEventPublisher(IPublishEndpoint publishEndpoint) =>
        _publishEndpoint = publishEndpoint;

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IntegrationEvent =>
        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
}

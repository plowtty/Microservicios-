namespace Orders.Application.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Domain.Events;
using Shared.EventBus.Events;
using Shared.EventBus.Interfaces;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(IEventBus eventBus, ILogger<OrderCreatedEventHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing OrderCreatedIntegrationEvent for Order {OrderId}", notification.OrderId);

        var integrationEvent = new OrderCreatedIntegrationEvent(
            notification.OrderId,
            notification.CustomerId,
            0,
            []);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

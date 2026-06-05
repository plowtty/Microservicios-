namespace Shared.EventBus.Events;

public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    List<OrderItemData> Items
) : IntegrationEvent;

public record OrderItemData(Guid ProductId, int Quantity, decimal UnitPrice);

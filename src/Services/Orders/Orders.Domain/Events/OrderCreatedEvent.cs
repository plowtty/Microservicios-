namespace Orders.Domain.Events;

using Shared.Common;

public record OrderCreatedEvent(Guid OrderId, Guid CustomerId) : DomainEvent;

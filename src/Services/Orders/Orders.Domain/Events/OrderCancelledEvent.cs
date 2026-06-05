namespace Orders.Domain.Events;

using Shared.Common;

public record OrderCancelledEvent(Guid OrderId, Guid CustomerId, string Reason) : DomainEvent;

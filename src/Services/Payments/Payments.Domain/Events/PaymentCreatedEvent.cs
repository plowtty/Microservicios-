namespace Payments.Domain.Events;

using Shared.Common;

public record PaymentCreatedEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;

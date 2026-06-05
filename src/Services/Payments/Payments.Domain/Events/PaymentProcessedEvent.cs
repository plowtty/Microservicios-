namespace Payments.Domain.Events;

using Shared.Common;

public record PaymentProcessedEvent(
    Guid PaymentId,
    Guid OrderId,
    bool IsSuccessful,
    string? FailureReason) : DomainEvent;

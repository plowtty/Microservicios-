namespace Shared.EventBus.Events;

public record PaymentProcessedIntegrationEvent(
    Guid OrderId,
    Guid PaymentId,
    bool IsSuccessful,
    string? FailureReason
) : IntegrationEvent;

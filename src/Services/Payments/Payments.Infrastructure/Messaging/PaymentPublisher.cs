namespace Payments.Infrastructure.Messaging;

using MassTransit;
using Payments.Application.Interfaces;
using Shared.EventBus.Events;

public class PaymentPublisher : IPaymentPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentPublisher(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublishPaymentProcessedAsync(
        Guid orderId,
        Guid paymentId,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken = default)
    {
        return _publishEndpoint.Publish(
            new PaymentProcessedIntegrationEvent(orderId, paymentId, isSuccessful, failureReason),
            cancellationToken);
    }
}

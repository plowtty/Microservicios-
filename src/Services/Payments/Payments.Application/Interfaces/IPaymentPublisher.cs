namespace Payments.Application.Interfaces;

public interface IPaymentPublisher
{
    Task PublishPaymentProcessedAsync(
        Guid orderId,
        Guid paymentId,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken = default);
}

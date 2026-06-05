namespace Orders.Infrastructure.Messaging.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using Orders.Application.Interfaces;
using Shared.EventBus.Events;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(IOrderRepository orderRepository, ILogger<PaymentProcessedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received PaymentProcessed for Order {OrderId}, Success: {IsSuccessful}",
            message.OrderId, message.IsSuccessful);

        var order = await _orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found when processing payment result", message.OrderId);
            return;
        }

        if (message.IsSuccessful)
            order.Confirm();
        else
            order.Cancel(message.FailureReason ?? "Payment failed");

        await _orderRepository.UpdateAsync(order, context.CancellationToken);
    }
}

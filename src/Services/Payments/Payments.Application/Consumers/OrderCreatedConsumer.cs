namespace Payments.Application.Consumers;

using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.Commands.ProcessPayment;
using Shared.EventBus.Events;

public class OrderCreatedConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IMediator mediator, ILogger<OrderCreatedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing payment for Order {OrderId}, Amount: {Amount}",
            message.OrderId, message.TotalAmount);

        var command = new ProcessPaymentCommand(
            message.OrderId,
            message.CustomerId,
            message.TotalAmount,
            "USD");

        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Payment {PaymentId} processed for Order {OrderId}", result.Value, message.OrderId);
        else
            _logger.LogError("Payment failed for Order {OrderId}: {Error}", message.OrderId, result.Error);
    }
}

namespace Orders.Application.Commands.CancelOrder;

using MediatR;
using Orders.Application.Interfaces;
using Orders.Domain.Exceptions;
using Shared.Common;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderHandler(IOrderRepository orderRepository) =>
        _orderRepository = orderRepository;

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure($"Order '{request.OrderId}' not found.");

        try
        {
            order.Cancel(request.Reason);
        }
        catch (InvalidOrderStateException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return Result.Success();
    }
}

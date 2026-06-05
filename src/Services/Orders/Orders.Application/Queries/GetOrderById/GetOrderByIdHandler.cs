namespace Orders.Application.Queries.GetOrderById;

using AutoMapper;
using MediatR;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Shared.Common;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        return order is null
            ? Result<OrderDto>.Failure($"Order '{request.OrderId}' not found.")
            : Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
}

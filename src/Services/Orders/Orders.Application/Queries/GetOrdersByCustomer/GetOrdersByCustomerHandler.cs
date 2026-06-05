namespace Orders.Application.Queries.GetOrdersByCustomer;

using AutoMapper;
using MediatR;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Shared.Common;

public class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<IEnumerable<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<OrderDto>>> Handle(
        GetOrdersByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        return Result<IEnumerable<OrderDto>>.Success(_mapper.Map<IEnumerable<OrderDto>>(orders));
    }
}

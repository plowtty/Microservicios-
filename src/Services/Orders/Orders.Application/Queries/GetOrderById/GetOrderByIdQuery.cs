namespace Orders.Application.Queries.GetOrderById;

using MediatR;
using Orders.Application.DTOs;
using Shared.Common;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;

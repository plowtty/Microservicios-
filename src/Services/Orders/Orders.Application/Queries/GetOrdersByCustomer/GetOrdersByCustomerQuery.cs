namespace Orders.Application.Queries.GetOrdersByCustomer;

using MediatR;
using Orders.Application.DTOs;
using Shared.Common;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<Result<IEnumerable<OrderDto>>>;

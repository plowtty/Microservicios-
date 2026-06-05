namespace Orders.Application.Commands.CreateOrder;

using MediatR;
using Orders.Application.DTOs;
using Shared.Common;

public record CreateOrderCommand(
    Guid CustomerId,
    AddressDto ShippingAddress,
    List<CreateOrderItemRequest> Items
) : IRequest<Result<Guid>>;

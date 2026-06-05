namespace Orders.Application.Commands.CancelOrder;

using MediatR;
using Shared.Common;

public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<Result>;

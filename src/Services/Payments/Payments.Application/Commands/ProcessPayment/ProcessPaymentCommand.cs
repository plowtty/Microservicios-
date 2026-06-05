namespace Payments.Application.Commands.ProcessPayment;

using MediatR;
using Shared.Common;

public record ProcessPaymentCommand(Guid OrderId, Guid CustomerId, decimal Amount, string Currency)
    : IRequest<Result<Guid>>;

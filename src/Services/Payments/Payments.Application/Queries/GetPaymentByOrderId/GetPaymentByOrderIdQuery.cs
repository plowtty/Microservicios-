namespace Payments.Application.Queries.GetPaymentByOrderId;

using MediatR;
using Shared.Common;
using Payments.Application.DTOs;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<Result<PaymentDto>>;

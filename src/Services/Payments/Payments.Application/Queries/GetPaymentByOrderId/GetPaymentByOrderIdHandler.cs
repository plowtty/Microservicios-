namespace Payments.Application.Queries.GetPaymentByOrderId;

using MediatR;
using Shared.Common;
using Payments.Application.DTOs;
using Payments.Domain.Interfaces;

public class GetPaymentByOrderIdHandler : IRequestHandler<GetPaymentByOrderIdQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdHandler(IPaymentRepository repository) => _repository = repository;

    public async Task<Result<PaymentDto>> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (payment is null)
            return Result<PaymentDto>.Failure($"Payment for order {request.OrderId} not found");

        return Result<PaymentDto>.Success(new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.Amount.Value,
            payment.Amount.Currency,
            payment.Status.ToString(),
            payment.FailureReason,
            payment.CreatedAt,
            payment.ProcessedAt));
    }
}

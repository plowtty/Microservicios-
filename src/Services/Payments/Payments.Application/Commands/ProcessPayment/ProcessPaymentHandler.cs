namespace Payments.Application.Commands.ProcessPayment;

using MediatR;
using Shared.Common;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Interfaces;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentPublisher _publisher;

    public ProcessPaymentHandler(IPaymentRepository repository, IPaymentPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = Payment.Create(request.OrderId, request.CustomerId, request.Amount, request.Currency);

        // Simulate: succeed if amount < 10000
        bool succeeded = request.Amount < 10_000m;

        if (succeeded)
            payment.MarkSucceeded();
        else
            payment.MarkFailed("Amount exceeds transaction limit");

        await _repository.AddAsync(payment, cancellationToken);

        await _publisher.PublishPaymentProcessedAsync(
            request.OrderId,
            payment.Id,
            succeeded,
            payment.FailureReason,
            cancellationToken);

        return Result<Guid>.Success(payment.Id);
    }
}

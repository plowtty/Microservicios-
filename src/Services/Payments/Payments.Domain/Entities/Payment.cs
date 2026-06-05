namespace Payments.Domain.Entities;

using Shared.Common;
using Payments.Domain.Enums;
using Payments.Domain.Events;
using Payments.Domain.ValueObjects;

public class Payment : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public PaymentAmount Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, Guid customerId, decimal amount, string currency)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = new PaymentAmount(amount, currency),
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        payment.RaiseDomainEvent(new PaymentCreatedEvent(payment.Id, orderId, amount));
        return payment;
    }

    public void MarkSucceeded()
    {
        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PaymentProcessedEvent(Id, OrderId, true, null));
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PaymentProcessedEvent(Id, OrderId, false, reason));
    }
}

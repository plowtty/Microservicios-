namespace Payments.UnitTests.Domain;

using FluentAssertions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Xunit;

public class PaymentTests
{
    private static Payment CreatePayment(decimal amount = 500m) =>
        Payment.Create(Guid.NewGuid(), Guid.NewGuid(), amount, "USD");

    [Fact]
    public void Create_ShouldSetPendingStatus()
    {
        var payment = CreatePayment();
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.ProcessedAt.Should().BeNull();
        payment.FailureReason.Should().BeNull();
        payment.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_ShouldStoreAmountAndCurrency()
    {
        var payment = CreatePayment(1234.56m);
        payment.Amount.Value.Should().Be(1234.56m);
        payment.Amount.Currency.Should().Be("USD");
    }

    [Fact]
    public void MarkSucceeded_ShouldSetSucceededStatus()
    {
        var payment = CreatePayment();
        payment.ClearDomainEvents();

        payment.MarkSucceeded();

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ProcessedAt.Should().NotBeNull();
        payment.FailureReason.Should().BeNull();
        payment.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void MarkFailed_ShouldSetFailedStatusWithReason()
    {
        var payment = CreatePayment();
        payment.ClearDomainEvents();

        payment.MarkFailed("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Insufficient funds");
        payment.ProcessedAt.Should().NotBeNull();
        payment.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        var act = () => Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 0m, "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Payment.Create(Guid.NewGuid(), Guid.NewGuid(), -50m, "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrow()
    {
        var act = () => Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m, "");
        act.Should().Throw<ArgumentException>();
    }
}

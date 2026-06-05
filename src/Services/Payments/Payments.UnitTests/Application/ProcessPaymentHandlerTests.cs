namespace Payments.UnitTests.Application;

using FluentAssertions;
using Moq;
using Payments.Application.Commands.ProcessPayment;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Interfaces;
using Xunit;

public class ProcessPaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _repoMock = new();
    private readonly Mock<IPaymentPublisher> _publisherMock = new();

    private ProcessPaymentHandler CreateHandler() =>
        new(_repoMock.Object, _publisherMock.Object);

    [Fact]
    public async Task Handle_WhenAmountUnderLimit_ShouldSucceed()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 500m, "USD");

        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _repoMock.Verify(r => r.AddAsync(
            It.Is<Payment>(p => p.Status == Payments.Domain.Enums.PaymentStatus.Succeeded),
            default), Times.Once);

        _publisherMock.Verify(p => p.PublishPaymentProcessedAsync(
            command.OrderId, It.IsAny<Guid>(), true, null, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAmountOverLimit_ShouldFail()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 15_000m, "USD");

        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue(); // handler always returns Success(paymentId)

        _repoMock.Verify(r => r.AddAsync(
            It.Is<Payment>(p => p.Status == Payments.Domain.Enums.PaymentStatus.Failed),
            default), Times.Once);

        _publisherMock.Verify(p => p.PublishPaymentProcessedAsync(
            command.OrderId, It.IsAny<Guid>(), false, It.IsNotNull<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_AlwaysPublishesEvent()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, "USD");

        await CreateHandler().Handle(command, default);

        _publisherMock.Verify(p => p.PublishPaymentProcessedAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string?>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_AlwaysPersistsPayment()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, "USD");

        await CreateHandler().Handle(command, default);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Once);
    }
}

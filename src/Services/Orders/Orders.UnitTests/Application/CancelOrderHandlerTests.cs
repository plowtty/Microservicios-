namespace Orders.UnitTests.Application;

using FluentAssertions;
using Moq;
using Orders.Application.Commands.CancelOrder;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.ValueObjects;
using Xunit;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WhenOrderExists_ShouldCancel()
    {
        var order = Order.Create(CustomerId.New(), new Address("St", "City", "State", "USA", "00000"));
        order.AddItem(OrderItem.Create(Guid.NewGuid(), "Product", 1, new Money(10m, "USD")));
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        var result = await new CancelOrderHandler(_repositoryMock.Object)
            .Handle(new CancelOrderCommand(order.Id, "Changed mind"), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Orders.Domain.Entities.Order?)null);

        var result = await new CancelOrderHandler(_repositoryMock.Object)
            .Handle(new CancelOrderCommand(Guid.NewGuid(), "reason"), default);

        result.IsSuccess.Should().BeFalse();
    }
}

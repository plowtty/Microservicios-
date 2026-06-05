namespace Orders.UnitTests.Application;

using FluentAssertions;
using Moq;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Xunit;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();

    private CreateOrderHandler CreateHandler() =>
        new(_repositoryMock.Object, _productServiceMock.Object);

    [Fact]
    public async Task Handle_WhenStockAvailable_ShouldCreateOrder()
    {
        var productId = Guid.NewGuid();

        _productServiceMock.Setup(s => s.CheckStockAsync(It.IsAny<IEnumerable<StockCheckItem>>(), default))
            .ReturnsAsync(true);
        _productServiceMock.Setup(s => s.GetProductDetailsAsync(productId, default))
            .ReturnsAsync(new ProductDetails(productId, "Test Product", 25m, true));

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new AddressDto("123 St", "City", "State", "USA", "00000"),
            [new CreateOrderItemRequest(productId, 2)]);

        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Orders.Domain.Entities.Order>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStockUnavailable_ShouldReturnFailure()
    {
        _productServiceMock.Setup(s => s.CheckStockAsync(It.IsAny<IEnumerable<StockCheckItem>>(), default))
            .ReturnsAsync(false);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new AddressDto("123 St", "City", "State", "USA", "00000"),
            [new CreateOrderItemRequest(Guid.NewGuid(), 2)]);

        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("out of stock");
    }
}

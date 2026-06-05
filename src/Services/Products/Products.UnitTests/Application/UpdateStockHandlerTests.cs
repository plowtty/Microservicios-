namespace Products.UnitTests.Application;

using FluentAssertions;
using Moq;
using Products.Application.Commands.UpdateStock;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Domain.ValueObjects;
using Xunit;

public class UpdateStockHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();

    private UpdateStockHandler CreateHandler() => new(_repoMock.Object);

    private static Product MakeProduct(int stock) =>
        Product.Create("P", "D", "C", new Money(10m, "USD"), stock);

    [Fact]
    public async Task Handle_AddOperation_ShouldIncreaseStock()
    {
        var product = MakeProduct(5);
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await CreateHandler().Handle(new UpdateStockCommand(product.Id, 10, StockOperation.Add), default);

        result.IsSuccess.Should().BeTrue();
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ReduceOperation_ShouldDecreaseStock()
    {
        var product = MakeProduct(20);
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await CreateHandler().Handle(new UpdateStockCommand(product.Id, 5, StockOperation.Reduce), default);

        result.IsSuccess.Should().BeTrue();
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ReduceMoreThanStock_ShouldReturnFailure()
    {
        var product = MakeProduct(3);
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await CreateHandler().Handle(new UpdateStockCommand(product.Id, 10, StockOperation.Reduce), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("stock");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailure()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        var result = await CreateHandler().Handle(new UpdateStockCommand(Guid.NewGuid(), 5, StockOperation.Add), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}

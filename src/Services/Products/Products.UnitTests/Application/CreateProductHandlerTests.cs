namespace Products.UnitTests.Application;

using FluentAssertions;
using Moq;
using Products.Application.Commands.CreateProduct;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Xunit;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();

    private CreateProductHandler CreateHandler() => new(_repoMock.Object);

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProductAndReturnId()
    {
        var command = new CreateProductCommand("Laptop", "Description", "Electronics", 999m, "USD", 10);

        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Name == "Laptop" && p.StockQuantity == 10), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassAllFieldsToProduct()
    {
        var command = new CreateProductCommand("Mouse", "Wireless", "Peripherals", 29.99m, "USD", 50);

        await CreateHandler().Handle(command, default);

        _repoMock.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Category == "Peripherals" &&
            p.Price.Amount == 29.99m &&
            p.Price.Currency == "USD"), default), Times.Once);
    }
}

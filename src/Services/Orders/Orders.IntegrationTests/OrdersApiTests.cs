namespace Orders.IntegrationTests;

using FluentAssertions;
using Orders.IntegrationTests.Fixtures;
using System.Net;
using Xunit;

public class OrdersApiTests : IClassFixture<OrdersWebAppFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(OrdersWebAppFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetOrder_WhenNotExists_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

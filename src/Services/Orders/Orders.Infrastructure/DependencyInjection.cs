namespace Orders.Infrastructure;

using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Interfaces;
using Orders.Infrastructure.GrpcClients;
using Orders.Infrastructure.Messaging.Consumers;
using Orders.Infrastructure.Messaging.Publishers;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Persistence.Repositories;
using Products.Grpc;
using Shared.EventBus.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("OrdersDb")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IEventBus, OrderEventPublisher>();

        services.AddGrpcClient<ProductService.ProductServiceClient>(o =>
            o.Address = new Uri(configuration["GrpcClients:Products"]!));

        services.AddScoped<IProductService, ProductGrpcClient>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentProcessedConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}

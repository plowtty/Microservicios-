namespace Products.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Interfaces;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Persistence.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ProductsDb")));

        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }
}

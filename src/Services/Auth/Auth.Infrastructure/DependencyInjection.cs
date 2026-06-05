namespace Auth.Infrastructure;

using Auth.Application.Interfaces;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("AuthDb")));

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Singleton: generates the RSA key pair once at startup
        services.AddSingleton<JwtTokenService>();
        services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<JwtTokenService>());

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();

        // JWT Bearer — configured lazily via IConfigureNamedOptions so the RSA key is ready
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddSingleton<
            Microsoft.Extensions.Options.IConfigureNamedOptions<JwtBearerOptions>,
            JwtBearerConfigureOptions>();

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
            .AddPolicy("CustomerOrAdmin", p => p.RequireRole("Customer", "Admin"));

        return services;
    }
}

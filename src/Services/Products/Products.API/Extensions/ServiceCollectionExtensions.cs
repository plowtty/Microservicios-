namespace Products.API.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Products Service", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT from Auth Service (RS256). Obtain via POST /api/auth/login"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });

        var jwksUri = configuration["Auth:JwksUri"] ?? "http://auth-service:8080/.well-known/jwks.json";
        var issuer   = configuration["Auth:Issuer"]  ?? "auth-service";
        var audience = configuration["Auth:Audience"] ?? "microservices";

        var keyProvider = new JwksKeyProvider(jwksUri);
        services.AddSingleton(keyProvider);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeyResolver = (_, _, kid, _) => keyProvider.GetKeys(kid)
                };
            });

        services.AddAuthorization();
        services.AddGrpc();
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("ProductsDb")!);

        return services;
    }
}

internal sealed class JwksKeyProvider
{
    private readonly string _jwksUri;
    private readonly HttpClient _http = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IList<JsonWebKey> _cachedKeys = [];
    private DateTime _expiresAt = DateTime.MinValue;

    public JwksKeyProvider(string jwksUri) => _jwksUri = jwksUri;

    public IEnumerable<SecurityKey> GetKeys(string? kid)
    {
        var keys = GetCachedKeysAsync().GetAwaiter().GetResult();
        return kid is not null ? keys.Where(k => k.KeyId == kid) : keys;
    }

    private async Task<IList<JsonWebKey>> GetCachedKeysAsync()
    {
        if (_cachedKeys.Count > 0 && DateTime.UtcNow < _expiresAt)
            return _cachedKeys;

        await _lock.WaitAsync();
        try
        {
            if (_cachedKeys.Count > 0 && DateTime.UtcNow < _expiresAt)
                return _cachedKeys;

            var json = await _http.GetStringAsync(_jwksUri);
            _cachedKeys = new JsonWebKeySet(json).Keys;
            _expiresAt = DateTime.UtcNow.AddHours(1);
        }
        catch
        {
            if (_cachedKeys.Count == 0) throw;
        }
        finally
        {
            _lock.Release();
        }

        return _cachedKeys;
    }
}

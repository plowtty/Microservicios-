namespace Auth.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Configures JwtBearerOptions lazily so that JwtTokenService (singleton) is
/// already available when the options are first accessed — avoids BuildServiceProvider().
/// </summary>
public class JwtBearerConfigureOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtTokenService _tokenService;
    private readonly JwtSettings _settings;

    public JwtBearerConfigureOptions(JwtTokenService tokenService, IOptions<JwtSettings> settings)
    {
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    public void Configure(JwtBearerOptions options) => Configure(string.Empty, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _tokenService.GetPublicKey(),
            ClockSkew = TimeSpan.Zero
        };
    }
}

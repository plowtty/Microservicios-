namespace Auth.Infrastructure.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtSettings
{
    public string Issuer { get; init; } = "auth-service";
    public string Audience { get; init; } = "microservices";
    public int AccessTokenExpiryMinutes { get; init; } = 15;
    public int RefreshTokenExpiryDays { get; init; } = 7;
    /// <summary>Base64-encoded RSA private key PEM. Generated on startup if empty (dev only).</summary>
    public string? RsaPrivateKeyBase64 { get; init; }
}

public class JwtTokenService : ITokenService, IDisposable
{
    private readonly JwtSettings _settings;
    private readonly RSA _rsa;
    private readonly RsaSecurityKey _privateKey;
    private readonly RsaSecurityKey _publicKey;
    private readonly string _keyId;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _rsa = RSA.Create(2048);

        if (!string.IsNullOrEmpty(_settings.RsaPrivateKeyBase64))
        {
            var pem = Encoding.UTF8.GetString(Convert.FromBase64String(_settings.RsaPrivateKeyBase64));
            _rsa.ImportFromPem(pem);
        }
        // else: use the freshly generated ephemeral key (dev)

        var rsaForPublic = RSA.Create();
        rsaForPublic.ImportRSAPublicKey(_rsa.ExportRSAPublicKey(), out _);

        _privateKey = new RsaSecurityKey(_rsa) { KeyId = ComputeKeyId() };
        _publicKey = new RsaSecurityKey(rsaForPublic) { KeyId = _privateKey.KeyId };
        _keyId = _privateKey.KeyId;
    }

    public TokenPairDto GenerateTokenPair(User user)
    {
        var jti = Guid.NewGuid().ToString();
        var accessExpiry = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes);
        var refreshExpiry = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var credentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: accessExpiry,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshTokenValue = GenerateRefreshTokenValue();

        return new TokenPairDto(accessToken, jti, accessExpiry, refreshTokenValue, refreshExpiry);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public IEnumerable<object> GetJsonWebKeys()
    {
        var parameters = _rsa.ExportParameters(false);
        yield return new
        {
            kty = "RSA",
            use = "sig",
            alg = "RS256",
            kid = _keyId,
            n = Base64UrlEncode(parameters.Modulus!),
            e = Base64UrlEncode(parameters.Exponent!)
        };
    }

    public RsaSecurityKey GetPublicKey() => _publicKey;

    private string ComputeKeyId()
    {
        var pub = _rsa.ExportRSAPublicKey();
        return Convert.ToBase64String(SHA256.HashData(pub))[..16];
    }

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public void Dispose() => _rsa.Dispose();
}

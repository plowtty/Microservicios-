namespace Auth.Application.Interfaces;

using Auth.Application.DTOs;
using Auth.Domain.Entities;

public interface ITokenService
{
    TokenPairDto GenerateTokenPair(User user);
    string HashToken(string rawToken);
    string GenerateRefreshTokenValue();
    /// <summary>Returns the RSA public key in JWK format for the JWKS endpoint.</summary>
    IEnumerable<object> GetJsonWebKeys();
}

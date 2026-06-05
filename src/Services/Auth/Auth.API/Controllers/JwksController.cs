namespace Auth.API.Controllers;

using Auth.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Exposes the RSA public key in JSON Web Key Set format so other services can
/// validate JWT signatures without sharing a secret.
/// </summary>
[ApiController]
public class JwksController : ControllerBase
{
    private readonly JwtTokenService _tokenService;

    public JwksController(JwtTokenService tokenService) => _tokenService = tokenService;

    [HttpGet(".well-known/jwks.json")]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetJwks() => Ok(new { keys = _tokenService.GetJsonWebKeys() });
}

namespace Auth.Infrastructure.Security;

using Auth.Application.Interfaces;

public class BcryptPasswordHasher : IPasswordHasher
{
    // Work factor 12: ~300ms per hash — strong against brute force
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

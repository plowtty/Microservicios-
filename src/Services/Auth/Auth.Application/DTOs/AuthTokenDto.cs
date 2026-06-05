namespace Auth.Application.DTOs;

public record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    DateTime RefreshTokenExpiry,
    string TokenType = "Bearer");

public record TokenPairDto(
    string AccessToken,
    string Jti,
    DateTime AccessTokenExpiry,
    string RefreshTokenValue,
    DateTime RefreshTokenExpiry);

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

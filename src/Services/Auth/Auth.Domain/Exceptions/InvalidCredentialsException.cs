namespace Auth.Domain.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string message = "Invalid credentials.") : base(message) { }
}

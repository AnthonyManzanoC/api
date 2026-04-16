namespace LamillaEscudero.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(string userId, string userName, string email, string fullName, IEnumerable<string> roles);
}
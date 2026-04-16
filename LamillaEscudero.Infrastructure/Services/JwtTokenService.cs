using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LamillaEscudero.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LamillaEscudero.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(string userId, string userName, string email, string fullName, IEnumerable<string> roles)
    {
        var section = _configuration.GetSection("Jwt");
        var key = section["Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada.");
        var issuer = section["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
        var audience = section["Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado.");
        var expiresMinutes = int.Parse(section["ExpiresMinutes"] ?? "120");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new("fullName", fullName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Auth;
using LamillaEscudero.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Credenciales inválidas." });

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Unauthorized(new { message = "Credenciales inválidas." });

        var roles = await _userManager.GetRolesAsync(user);

        // Aquí pasamos las variables simples en lugar del objeto completo
        var token = _jwtTokenService.CreateToken(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName ?? string.Empty,
            roles);

        return Ok(new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            FullName = user.FullName ?? user.Email ?? string.Empty,
            Roles = roles
        });
    }
}
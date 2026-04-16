using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Cuentas;
using LamillaEscudero.Infrastructure.Identity;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class CentroCuentasService : ICentroCuentasService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager; // ✅ NUEVO: Gestor de Roles
    private readonly AppDbContext _ctx;

    public CentroCuentasService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AppDbContext ctx)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _ctx = ctx;
    }

    public async Task<List<UsuarioResponse>> GetAllUsuariosAsync(CancellationToken ct = default)
    {
        var users = await _userManager.Users.ToListAsync(ct);
        var result = new List<UsuarioResponse>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);

            // Buscar cliente vinculado si es rol Cliente
            Guid? clienteId = null;
            string? clienteNombre = null;
            if (roles.Contains("Cliente"))
            {
                var cliente = await _ctx.Clientes.FirstOrDefaultAsync(c => c.UserId == u.Id, ct);
                if (cliente is not null)
                {
                    clienteId = cliente.Id;
                    clienteNombre = cliente.Nombres;
                }
            }

            result.Add(new UsuarioResponse
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                Cedula = u.Cedula,
                IsActive = u.IsActive,
                Roles = roles.ToList(),
                ClienteVinculadoId = clienteId,
                ClienteVinculadoNombre = clienteNombre
            });
        }

        return result.OrderBy(x => x.Email).ToList();
    }

    public async Task<(bool Success, string? Error)> CrearUsuarioAsync(
        CrearUsuarioRequest request, CancellationToken ct = default)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(request.Email))
            return (false, "El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return (false, "La contraseña debe tener al menos 8 caracteres.");

        var rolesPermitidos = new[] { "Administrador", "Asistente", "Cliente", "Abogado" };
        if (!rolesPermitidos.Contains(request.Rol))
            return (false, $"Rol '{request.Rol}' no permitido.");

        if (request.Rol == "Cliente" && !request.ClienteId.HasValue)
            return (false, "Debes seleccionar el cliente a vincular.");

        // Crear usuario
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName.Trim(),
            Cedula = request.Cedula?.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        // ✅ SOLUCIÓN: Verifica si el rol existe en la BD. Si no existe, lo crea al instante.
        if (!await _roleManager.RoleExistsAsync(request.Rol))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = request.Rol });
        }

        // Asignar el rol al usuario
        await _userManager.AddToRoleAsync(user, request.Rol);

        // Vincular cliente si es necesario
        if (request.Rol == "Cliente" && request.ClienteId.HasValue)
        {
            var cliente = await _ctx.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId.Value, ct);
            if (cliente is not null)
            {
                cliente.UserId = user.Id;
                await _ctx.SaveChangesAsync(ct);
            }
        }

        return (true, null);
    }

    public async Task<bool> ToggleActivoAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> EliminarUsuarioAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        // Desvincula clientes asociados
        var clientes = await _ctx.Clientes.Where(c => c.UserId == userId).ToListAsync(ct);
        foreach (var c in clientes)
        {
            c.UserId = null;
        }
        await _ctx.SaveChangesAsync(ct);

        await _userManager.DeleteAsync(user);
        return true;
    }
}
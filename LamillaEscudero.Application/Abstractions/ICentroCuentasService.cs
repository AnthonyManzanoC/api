

using LamillaEscudero.Application.Models.Cuentas;

namespace LamillaEscudero.Application.Abstractions;

public interface ICentroCuentasService
{
    Task<List<UsuarioResponse>> GetAllUsuariosAsync(CancellationToken ct = default);
    Task<(bool Success, string? Error)> CrearUsuarioAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task<bool> ToggleActivoAsync(string userId, CancellationToken ct = default);
    Task<bool> EliminarUsuarioAsync(string userId, CancellationToken ct = default);
}


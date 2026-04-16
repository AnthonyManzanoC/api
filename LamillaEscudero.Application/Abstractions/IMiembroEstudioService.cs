using LamillaEscudero.Application.Models.Miembros;
using LamillaEscudero.Application.Models.Servicios;
using LamillaEscudero.Application.Models.Cuentas;

namespace LamillaEscudero.Application.Abstractions;

public interface IMiembroEstudioService
{
    Task<List<MiembroEstudioResponse>> GetAllAsync(CancellationToken ct = default);
    Task<MiembroEstudioResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MiembroEstudioResponse> CreateAsync(MiembroEstudioCreateRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, MiembroEstudioUpdateRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}


using LamillaEscudero.Application.Models.Abonos;
using LamillaEscudero.Application.Models.Clientes;

namespace LamillaEscudero.Application.Abstractions;

public interface IClienteService
{
    // ── CRUD base ─────────────────────────────────────────────
    Task<List<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteResponse> CreateAsync(ClienteCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, ClienteUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // ✅ FASE 10 — Honorarios y Abonos ─────────────────────────
    Task<bool> UpdateHonorariosAsync(Guid clienteId, decimal totalHonorarios, CancellationToken cancellationToken = default);
    Task<AbonoResponse> AddAbonoAsync(AbonoCreateRequest request, CancellationToken cancellationToken = default);
    Task<List<AbonoResponse>> GetAbonosByClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAbonoAsync(Guid abonoId, CancellationToken cancellationToken = default);
}
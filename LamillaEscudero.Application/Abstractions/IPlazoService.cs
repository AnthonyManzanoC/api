using LamillaEscudero.Application.Models.Plazos;

namespace LamillaEscudero.Application.Abstractions;

public interface IPlazoService
{
    Task<List<PlazoResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<PlazoResponse>> GetUpcomingAsync(int days = 7, CancellationToken cancellationToken = default);
    Task<List<PlazoResponse>> GetByEventoProcesalIdAsync(Guid eventoProcesalId, CancellationToken cancellationToken = default);
    Task<PlazoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PlazoResponse> CreateAsync(PlazoCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, PlazoUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> MarkCumplidoAsync(Guid id, bool cumplido, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
using LamillaEscudero.Application.Models.Causas;

namespace LamillaEscudero.Application.Abstractions;

public interface ICausaService
{
    Task<List<CausaResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<CausaResponse>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<CausaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CausaResponse> CreateAsync(CausaCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, CausaUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
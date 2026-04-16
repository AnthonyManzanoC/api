using LamillaEscudero.Application.Models.Eventos;

namespace LamillaEscudero.Application.Abstractions;

public interface IEventoProcesalService
{
    Task<List<EventoProcesalResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<EventoProcesalResponse>> GetByCausaIdAsync(Guid causaId, CancellationToken cancellationToken = default);
    Task<EventoProcesalResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventoProcesalResponse> CreateAsync(EventoProcesalCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, EventoProcesalUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
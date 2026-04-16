using LamillaEscudero.Application.Models.Servicios;

namespace LamillaEscudero.Application.Abstractions;

public interface IServicioOfrecidoService
{
    Task<List<ServicioOfrecidoResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ServicioOfrecidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServicioOfrecidoResponse> CreateAsync(ServicioOfrecidoCreateRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, ServicioOfrecidoUpdateRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

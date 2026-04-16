using LamillaEscudero.Application.Models.Consultas;

namespace LamillaEscudero.Application.Abstractions;

public interface IConsultaWebService
{
    Task<List<ConsultaWebResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ConsultaWebResponse>> GetUnReadAsync(CancellationToken cancellationToken = default);
    Task<ConsultaWebResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConsultaWebResponse> CreateAsync(ConsultaWebCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Actualiza estado CRM (estado, fecha cita, notas internas)</summary>
    Task<bool> UpdateAsync(Guid id, ConsultaWebUpdateRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

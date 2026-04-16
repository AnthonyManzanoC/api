using LamillaEscudero.Application.Models.Configuracion;

namespace LamillaEscudero.Application.Abstractions;

public interface IConfiguracionEstudioService
{
    Task<ConfiguracionEstudioResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, ConfiguracionEstudioUpdateRequest request, CancellationToken cancellationToken = default);
}
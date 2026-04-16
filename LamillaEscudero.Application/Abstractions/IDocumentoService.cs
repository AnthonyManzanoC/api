using LamillaEscudero.Application.Models.Documentos;

namespace LamillaEscudero.Application.Abstractions;

public interface IDocumentoService
{
    Task<List<DocumentoResponse>> GetByCausaIdAsync(Guid causaId, CancellationToken cancellationToken = default);
    Task<DocumentoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentoResponse> UploadAsync(Guid causaId, string nombre, Stream archivo, string fileName, string? contentType, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
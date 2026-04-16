namespace LamillaEscudero.Application.Models.Documentos;

public class DocumentoResponse
{
    public Guid Id { get; set; }
    public Guid CausaId { get; set; }
    public string NumeroProceso { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? TipoMime { get; set; }
    public long TamanoBytes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
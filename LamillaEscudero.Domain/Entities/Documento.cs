using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class Documento : BaseEntity
{
    public Guid CausaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? TipoMime { get; set; }
    public long TamanoBytes { get; set; }

    public Causa? Causa { get; set; }
}
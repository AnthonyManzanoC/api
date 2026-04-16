using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class EventoProcesal : BaseEntity
{
    public Guid CausaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaEvento { get; set; }
    public bool EsAutomatico { get; set; }
    public string? Fuente { get; set; }

    // ✅ NUEVO: indica si el abogado debe asistir físicamente (audiencia, versión, etc.)
    public bool RequiereAsistencia { get; set; }

    // ✅ NUEVO: fecha + hora exacta de la diligencia (a diferencia de los plazos que son fecha-día)
    public DateTime? FechaHoraAgendada { get; set; }

    public Causa? Causa { get; set; }
    public List<Plazo> Plazos { get; set; } = new();
}

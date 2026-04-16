namespace LamillaEscudero.Application.Models.Eventos;

public class EventoProcesalCreateRequest
{
    public Guid CausaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaEvento { get; set; }
    public bool EsAutomatico { get; set; }
    public string? Fuente { get; set; }

    // ✅ NUEVO: ¿El abogado debe asistir físicamente?
    public bool RequiereAsistencia { get; set; }

    // ✅ NUEVO: Fecha y hora exacta de la audiencia/diligencia/versión
    public DateTime? FechaHoraAgendada { get; set; }
}

namespace LamillaEscudero.Application.Models.Eventos;

public class EventoProcesalUpdateRequest
{
    public Guid CausaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaEvento { get; set; }
    public bool EsAutomatico { get; set; }
    public string? Fuente { get; set; }
    public bool RequiereAsistencia { get; set; }
    public DateTime? FechaHoraAgendada { get; set; }
    public bool IsActive { get; set; } = true;
}

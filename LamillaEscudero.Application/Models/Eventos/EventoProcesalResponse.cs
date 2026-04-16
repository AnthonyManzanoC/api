namespace LamillaEscudero.Application.Models.Eventos;

public class EventoProcesalResponse
{
    public Guid Id { get; set; }
    public Guid CausaId { get; set; }
    public string NumeroProceso { get; set; } = string.Empty;

    // Identificador visual de la causa (puede ser fiscal o proceso)
    public string IdentificadorCausa { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaEvento { get; set; }
    public bool EsAutomatico { get; set; }
    public string? Fuente { get; set; }
    public bool RequiereAsistencia { get; set; }
    public DateTime? FechaHoraAgendada { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Etiqueta para mostrar en UI
    public string EtiquetaTipo => RequiereAsistencia ? $"🏛 {Tipo}" : Tipo;
}

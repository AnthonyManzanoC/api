using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class Notificacion : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Sistema";
    public string? RelacionTipo { get; set; }
    public Guid? RelacionId { get; set; }
    public DateTime FechaProgramada { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEnviada { get; set; }
    public bool Enviada { get; set; }
    public bool Leida { get; set; }
}
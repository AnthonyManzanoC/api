namespace LamillaEscudero.Application.Models.Notificaciones;

public class NotificacionCreateRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Sistema";
    public string? RelacionTipo { get; set; }
    public Guid? RelacionId { get; set; }
    public DateTime? FechaProgramada { get; set; }
}
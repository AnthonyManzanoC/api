namespace LamillaEscudero.Application.Models.Notificaciones;

public class NotificacionResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? RelacionTipo { get; set; }
    public Guid? RelacionId { get; set; }
    public DateTime FechaProgramada { get; set; }
    public DateTime? FechaEnviada { get; set; }
    public bool Enviada { get; set; }
    public bool Leida { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
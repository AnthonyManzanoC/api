using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

/// <summary>
/// Testimonio enviado por un visitante del sitio web público.
/// Estado: Pendiente → revisión manual | Aprobado → visible en Home | Rechazado → descartado.
/// </summary>
public class Testimonio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Calificacion { get; set; } = 5;   // 1-5 estrellas
    public string Estado { get; set; } = EstadoTestimonio.Pendiente;
    public bool FlagHate { get; set; } = false;
    public string? NotaAdmin { get; set; }
}

public static class EstadoTestimonio
{
    public const string Pendiente = "Pendiente";
    public const string Aprobado = "Aprobado";
    public const string Rechazado = "Rechazado";
}

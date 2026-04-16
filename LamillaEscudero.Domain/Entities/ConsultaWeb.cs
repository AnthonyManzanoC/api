using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class ConsultaWeb : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>true = ya fue leída al menos una vez</summary>
    public bool Leida { get; set; }

    // ── CRM (Fase 7) ─────────────────────────────────────────────
    /// <summary>
    /// Pendiente | EnRevision | CitaAgendada | Respondida | Concluida
    /// </summary>
    public string Estado { get; set; } = EstadoConsulta.Pendiente;

    /// <summary>Fecha/hora propuesta o confirmada para la cita</summary>
    public DateTime? FechaCita { get; set; }

    /// <summary>Notas internas del abogado / asistente</summary>
    public string? NotasInternas { get; set; }
}

/// <summary>Constantes de estado para evitar magic-strings</summary>
public static class EstadoConsulta
{
    public const string Pendiente = "Pendiente";
    public const string EnRevision = "En Revisión";
    public const string CitaAgendada = "Cita Agendada";
    public const string Respondida = "Respondida";
    public const string Concluida = "Concluida";

    public static readonly string[] Todos =
    [
        Pendiente, EnRevision, CitaAgendada, Respondida, Concluida
    ];
}
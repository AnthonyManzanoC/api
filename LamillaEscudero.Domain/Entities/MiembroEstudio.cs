using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

/// <summary>
/// Miembro del equipo del estudio.
/// Fase 9: agrega PhotoData (Base64), FraseDestacada, BiografiaLarga,
/// EducacionJson y TimelineExperienciaJson (JSON serializados).
/// </summary>
public class MiembroEstudio : BaseEntity
{
    public string Nombres { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string? BiografiaBreve { get; set; }
    public string? BiografiaCompleta { get; set; }   // sigue siendo la preview corta en tarjeta

    // ── FASE 8 (URL externa, sigue siendo opcional) ───────────
    public string? FotoUrl { get; set; }

    // ── FASE 9: foto local como Base64 ────────────────────────
    /// <summary>
    /// Imagen subida localmente, guardada como data-URI Base64.
    /// Tiene prioridad sobre FotoUrl al renderizar.
    /// </summary>
    public string? PhotoData { get; set; }   // nvarchar(max)

    // ── FASE 9: perfil extendido ──────────────────────────────
    /// <summary>Frase/slogan personal mostrada en el overlay.</summary>
    public string? FraseDestacada { get; set; }   // nvarchar(max)

    /// <summary>Biografía larga mostrada en el overlay.</summary>
    public string? BiografiaLarga { get; set; }   // nvarchar(max)

    /// <summary>
    /// JSON: List&lt;EducacionItem&gt;
    /// [ { "Titulo": "...", "Institucion": "...", "Anio": "2010" }, … ]
    /// </summary>
    public string? EducacionJson { get; set; }   // nvarchar(max)

    /// <summary>
    /// JSON: List&lt;TimelineExperienciaItem&gt;
    /// [ { "Anio": "2015", "Titulo": "...", "Descripcion": "..." }, … ]
    /// </summary>
    public string? TimelineExperienciaJson { get; set; }   // nvarchar(max)

    // ── campos originales ─────────────────────────────────────
    public string? LinkedIn { get; set; }
    public string? Email { get; set; }
    public int Orden { get; set; } = 0;
}
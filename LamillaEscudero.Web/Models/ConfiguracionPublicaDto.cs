// ════════════════════════════════════════════════════════════════
//  LamillaEscudero.Web — Modelos y cliente para la API pública
//  Actualización Fase 7: agrega campos de mapa al DTO público
// ════════════════════════════════════════════════════════════════

// ── ConfiguracionPublicaDto.cs ───────────────────────────────────
namespace LamillaEscudero.Web.Models;

public class ConfiguracionPublicaDto
{
    public Guid Id { get; set; }
    public string NombreEstudio { get; set; } = string.Empty;
    public string? Slogan { get; set; }
    public string? LogoUrl { get; set; }
    public string? ColorPrimario { get; set; }
    public string? ColorSecundario { get; set; }
    public string? ColorFondo { get; set; }
    public string? EmailContacto { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? Direccion { get; set; }

    // ✅ NUEVO: campos de mapa
    public double? MapLatitude { get; set; }
    public double? MapLongitude { get; set; }
    public string? MapEmbedUrl { get; set; }
}
    
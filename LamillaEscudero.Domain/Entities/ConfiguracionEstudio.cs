using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class ConfiguracionEstudio : BaseEntity
{
    // ── Identidad del estudio ────────────────────────────────────
    public string NombreEstudio { get; set; } = "Lamilla Escudero & Asociados";
    public string? Slogan { get; set; }
    public string? LogoUrl { get; set; }

    // ✅ FASE 8: Foto/logo almacenado como Base64 o URL pública
    public string? PhotoUrl { get; set; }

    public string? ColorPrimario { get; set; } = "#0B1F3A";
    public string? ColorSecundario { get; set; } = "#D4AF37";
    public string? ColorFondo { get; set; } = "#F7F8FA";
    public string? EmailContacto { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? Direccion { get; set; }

    // ── SMTP (Fase 7) ────────────────────────────────────────────
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPass { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
    public string? SmtpFromName { get; set; }

    // ── Mapa / Ubicación (Fase 7) ────────────────────────────────
    public double? MapLatitude { get; set; }
    public double? MapLongitude { get; set; }
    public string? MapEmbedUrl { get; set; }
}
namespace LamillaEscudero.Application.Models.Configuracion;

public class ConfiguracionEstudioResponse
{
    public Guid Id { get; set; }
    public string NombreEstudio { get; set; } = string.Empty;
    public string? Slogan { get; set; }
    public string? LogoUrl { get; set; }
    public string? PhotoUrl { get; set; }   // ✅ FASE 8
    public string? ColorPrimario { get; set; }
    public string? ColorSecundario { get; set; }
    public string? ColorFondo { get; set; }
    public string? EmailContacto { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? Direccion { get; set; }
    public bool IsActive { get; set; }

    // SMTP
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPass { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
    public string? SmtpFromName { get; set; }

    // Mapa
    public double? MapLatitude { get; set; }
    public double? MapLongitude { get; set; }
    public string? MapEmbedUrl { get; set; }
}


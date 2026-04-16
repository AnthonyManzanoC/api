using System.Net;
using System.Net.Mail;
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LamillaEscudero.Infrastructure.Services;

/// <summary>
/// Servicio de correo que lee las credenciales SMTP directamente
/// de la tabla ConfiguracionesEstudio — nunca de appsettings.
/// </summary>
public class EmailService : IEmailService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmailService> _logger;

    public EmailService(AppDbContext db, ILogger<EmailService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Plantillas de negocio ─────────────────────────────────────

    public Task<bool> EnviarConfirmacionRecepcionAsync(
        string destinatario, string nombreCliente,
        string asunto, CancellationToken ct = default)
    {
        var html = PlantillaBase(
            titulo: "Consulta recibida",
            colorAccent: "#0B1F3A",
            icono: "✅",
            cuerpo: $"""
                <p>Estimado/a <strong>{HtmlEncode(nombreCliente)}</strong>,</p>
                <p>Hemos recibido su consulta sobre <em>«{HtmlEncode(asunto)}»</em>.</p>
                <p>Un miembro de nuestro equipo la revisará y se pondrá en contacto con usted a la brevedad.</p>
                <p>Gracias por confiar en <strong>Lamilla Escudero &amp; Asociados</strong>.</p>
            """);

        return EnviarAsync(destinatario, $"Consulta recibida — {asunto}", html, ct);
    }

    public Task<bool> EnviarRecordatorioCitaAsync(
        string destinatario, string nombreCliente,
        DateTime fechaCita, string asunto,
        CancellationToken ct = default)
    {
        var fechaStr = fechaCita.ToString("dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm",
                           new System.Globalization.CultureInfo("es-EC"));

        var html = PlantillaBase(
            titulo: "Recordatorio de cita",
            colorAccent: "#B8963E",
            icono: "📅",
            cuerpo: $"""
                <p>Estimado/a <strong>{HtmlEncode(nombreCliente)}</strong>,</p>
                <p>Le recordamos que tiene una cita programada en nuestro estudio:</p>
                <div style="background:#f8f6f1;border-left:4px solid #B8963E;padding:16px 20px;margin:20px 0;border-radius:4px;">
                    <strong>📌 Asunto:</strong> {HtmlEncode(asunto)}<br>
                    <strong>🗓 Fecha:</strong> {fechaStr}
                </div>
                <p>Si necesita reagendar, comuníquese con nosotros con antelación.</p>
            """);

        return EnviarAsync(destinatario, $"Recordatorio de cita — {asunto}", html, ct);
    }

    public Task<bool> EnviarRespuestaConsultaAsync(
        string destinatario, string nombreCliente,
        string asunto, string mensaje,
        CancellationToken ct = default)
    {
        var html = PlantillaBase(
            titulo: "Respuesta a su consulta",
            colorAccent: "#1D6A2E",
            icono: "💬",
            cuerpo: $"""
                <p>Estimado/a <strong>{HtmlEncode(nombreCliente)}</strong>,</p>
                <p>A continuación encontrará nuestra respuesta sobre <em>«{HtmlEncode(asunto)}»</em>:</p>
                <div style="background:#f0f9f1;border-left:4px solid #1D6A2E;padding:16px 20px;margin:20px 0;border-radius:4px;white-space:pre-wrap;">
                    {HtmlEncode(mensaje)}
                </div>
                <p>No dude en comunicarse si tiene dudas adicionales.</p>
            """);

        return EnviarAsync(destinatario, $"Respuesta a su consulta — {asunto}", html, ct);
    }

    public Task<bool> EnviarConfirmacionCitaAsync(
        string destinatario, string nombreCliente,
        DateTime fechaCita, string asunto,
        CancellationToken ct = default)
    {
        var fechaStr = fechaCita.ToString("dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm",
                           new System.Globalization.CultureInfo("es-EC"));

        var html = PlantillaBase(
            titulo: "Cita confirmada",
            colorAccent: "#0B6E4F",
            icono: "🎉",
            cuerpo: $"""
                <p>Estimado/a <strong>{HtmlEncode(nombreCliente)}</strong>,</p>
                <p>Nos complace confirmarle su cita en nuestro estudio jurídico:</p>
                <div style="background:#f0faf5;border-left:4px solid #0B6E4F;padding:16px 20px;margin:20px 0;border-radius:4px;">
                    <strong>📌 Asunto:</strong> {HtmlEncode(asunto)}<br>
                    <strong>✅ Fecha confirmada:</strong> {fechaStr}
                </div>
                <p>Por favor, preséntese 10 minutos antes de la hora indicada con su documento de identidad.</p>
            """);

        return EnviarAsync(destinatario, $"Cita confirmada — {asunto}", html, ct);
    }

    // ── Envío genérico ────────────────────────────────────────────

    public async Task<bool> EnviarAsync(
        string destinatario, string asunto,
        string htmlBody, CancellationToken ct = default)
    {
        try
        {
            var cfg = await _db.ConfiguracionesEstudio
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (cfg is null
                || string.IsNullOrWhiteSpace(cfg.SmtpServer)
                || string.IsNullOrWhiteSpace(cfg.SmtpUser)
                || string.IsNullOrWhiteSpace(cfg.SmtpPass))
            {
                _logger.LogWarning("SMTP no configurado en la base de datos. No se envió el correo a {To}.", destinatario);
                return false;
            }

            using var client = new SmtpClient(cfg.SmtpServer, cfg.SmtpPort)
            {
                Credentials = new NetworkCredential(cfg.SmtpUser, cfg.SmtpPass),
                EnableSsl = cfg.SmtpEnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15_000
            };

            var fromName = cfg.SmtpFromName ?? cfg.NombreEstudio;
            var fromAddress = new MailAddress(cfg.SmtpUser, fromName);

            using var msg = new MailMessage
            {
                From = fromAddress,
                Subject = asunto,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(destinatario);

            await client.SendMailAsync(msg, ct);

            _logger.LogInformation("Correo enviado a {To} — Asunto: {Subject}", destinatario, asunto);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo a {To}", destinatario);
            return false;
        }
    }

    // ── Plantilla HTML base ───────────────────────────────────────

    private static string PlantillaBase(string titulo, string colorAccent, string icono, string cuerpo) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f4f4f4;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:40px 16px;">
              <table width="600" cellpadding="0" cellspacing="0"
                     style="background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);">

                <!-- HEADER -->
                <tr>
                  <td style="background:{colorAccent};padding:32px 40px;text-align:center;">
                    <div style="font-size:2.5rem;margin-bottom:8px;">{icono}</div>
                    <h1 style="margin:0;color:#fff;font-size:1.5rem;font-weight:600;
                               font-family:Georgia,serif;letter-spacing:-.01em;">{titulo}</h1>
                    <p style="margin:8px 0 0;color:rgba(255,255,255,.7);font-size:.85rem;">
                        Lamilla Escudero &amp; Asociados
                    </p>
                  </td>
                </tr>

                <!-- CUERPO -->
                <tr>
                  <td style="padding:36px 40px;color:#1c1c1e;font-size:.95rem;line-height:1.7;">
                    {cuerpo}
                  </td>
                </tr>

                <!-- FOOTER -->
                <tr>
                  <td style="background:#f8f6f1;padding:20px 40px;text-align:center;
                             border-top:1px solid #e8e6e1;">
                    <p style="margin:0;color:#8a8a8e;font-size:.78rem;">
                      Este mensaje fue generado automáticamente por el sistema de gestión del estudio.<br>
                      Por favor no responda directamente a este correo.
                    </p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string HtmlEncode(string? s)
        => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
}

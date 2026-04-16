namespace LamillaEscudero.Application.Abstractions;

/// <summary>
/// Servicio de envío de correos electrónicos.
/// Las credenciales SMTP se obtienen dinámicamente desde
/// ConfiguracionesEstudio (base de datos), nunca desde appsettings.
/// </summary>
public interface IEmailService
{
    /// <summary>Envía confirmación de recepción al cliente</summary>
    Task<bool> EnviarConfirmacionRecepcionAsync(
        string destinatario, string nombreCliente,
        string asunto, CancellationToken ct = default);

    /// <summary>Envía recordatorio de cita al cliente</summary>
    Task<bool> EnviarRecordatorioCitaAsync(
        string destinatario, string nombreCliente,
        DateTime fechaCita, string asunto,
        CancellationToken ct = default);

    /// <summary>Envía respuesta/resolución de la consulta al cliente</summary>
    Task<bool> EnviarRespuestaConsultaAsync(
        string destinatario, string nombreCliente,
        string asunto, string mensaje,
        CancellationToken ct = default);

    /// <summary>Notifica que la cita está confirmada</summary>
    Task<bool> EnviarConfirmacionCitaAsync(
        string destinatario, string nombreCliente,
        DateTime fechaCita, string asunto,
        CancellationToken ct = default);

    /// <summary>Envío genérico con plantilla HTML personalizada</summary>
    Task<bool> EnviarAsync(
        string destinatario, string asunto,
        string htmlBody, CancellationToken ct = default);
}

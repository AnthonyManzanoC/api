using LamillaEscudero.Application.Models.Notificaciones;

namespace LamillaEscudero.Application.Abstractions;

public interface INotificacionService
{
    Task<List<NotificacionResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<NotificacionResponse>> GetUnreadAsync(CancellationToken cancellationToken = default);
    Task<NotificacionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificacionResponse> CreateAsync(NotificacionCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Genera notificaciones de tipo Recordatorio para plazos próximos a vencer.</summary>
    Task<int> GenerarRecordatoriosDePlazosAsync(int diasPorDelante = 3, CancellationToken cancellationToken = default);

    /// <summary>Genera notificaciones de tipo Vencido para plazos que ya expiraron sin cumplir.</summary>
    Task<int> GenerarRecordatoriosVencidosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ✅ NUEVO: Genera notificaciones de tipo Audiencia/Diligencia para eventos procesales
    /// que requieren asistencia física y están programados en los próximos días.
    /// </summary>
    Task<int> GenerarRecordatoriosDeAudienciasAsync(int diasPorDelante = 2, CancellationToken cancellationToken = default);
}

using LamillaEscudero.Application.Abstractions;

namespace LamillaEscudero.Infrastructure.Services;

public class AutomationService : IAutomationService
{
    private readonly INotificacionService _notificacionService;

    public AutomationService(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    public async Task EjecutarAsync(CancellationToken cancellationToken = default)
    {
        // 1. Recordatorios de plazos próximos (3 días)
        await _notificacionService.GenerarRecordatoriosDePlazosAsync(3, cancellationToken);

        // 2. Alertas de plazos vencidos sin cumplir
        await _notificacionService.GenerarRecordatoriosVencidosAsync(cancellationToken);

        // 3. ✅ NUEVO: Recordatorios de audiencias y diligencias (próximos 2 días)
        await _notificacionService.GenerarRecordatoriosDeAudienciasAsync(2, cancellationToken);
    }
}

using LamillaEscudero.Application.Models.Eventos;
using LamillaEscudero.Application.Models.Notificaciones;
using LamillaEscudero.Application.Models.Plazos;

namespace LamillaEscudero.Application.Models.Dashboard;

public class DashboardSummaryResponse
{
    // ── Métricas generales ─────────────────────────────────────
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int TotalCausas { get; set; }
    public int CausasActivas { get; set; }
    public int TotalEventos { get; set; }

    // ── Plazos ────────────────────────────────────────────────
    public int PlazosVencenHoy { get; set; }
    public int PlazosProximos7Dias { get; set; }

    // ── Audiencias ────────────────────────────────────────────
    public int AudienciasHoy { get; set; }
    public int AudienciasProximos7Dias { get; set; }

    // ── Notificaciones ────────────────────────────────────────
    public int ConsultasWebSinLeer { get; set; }
    public int NotificacionesSinLeer { get; set; }
    public int NotificacionesHoy { get; set; }

    // ✅ FASE 8: Métricas CRM de Consultas por estado ──────────
    public int ConsultasPendientes { get; set; }
    public int ConsultasEnRevision { get; set; }
    public int ConsultasCitasAgendadas { get; set; }
    public int ConsultasRespondidas { get; set; }
    public int ConsultasConcluidas { get; set; }
    public int ConsultasTotalActivas { get; set; }

    // ── Listas ────────────────────────────────────────────────
    public List<PlazoResponse> ProximosPlazos { get; set; } = new();
    public List<NotificacionResponse> UltimasNotificaciones { get; set; } = new();
    public List<EventoProcesalResponse> ProximasAudiencias { get; set; } = new();
}
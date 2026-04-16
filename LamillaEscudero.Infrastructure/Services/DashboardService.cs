using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Dashboard;
using LamillaEscudero.Application.Models.Eventos;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly IPlazoService _plazoService;

    public DashboardService(AppDbContext context, IPlazoService plazoService)
    {
        _context = context;
        _plazoService = plazoService;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var next7 = today.AddDays(7);

        // ── Generales ─────────────────────────────────────────────
        var totalClientes = await _context.Clientes.CountAsync(cancellationToken);
        var clientesActivos = await _context.Clientes.CountAsync(x => x.IsActive, cancellationToken);
        var totalCausas = await _context.Causas.CountAsync(cancellationToken);
        var causasActivas = await _context.Causas.CountAsync(x => x.IsActive, cancellationToken);
        var totalEventos = await _context.EventosProcesales.CountAsync(cancellationToken);

        // ── Plazos ────────────────────────────────────────────────
        var plazosVencenHoy = await _context.Plazos
            .CountAsync(x => x.IsActive && !x.Cumplido && x.FechaVencimiento.Date == today,
                        cancellationToken);
        var plazosProximos7 = await _context.Plazos
            .CountAsync(x => x.IsActive && !x.Cumplido
                          && x.FechaVencimiento.Date >= today
                          && x.FechaVencimiento.Date <= next7,
                        cancellationToken);

        // ── Audiencias ────────────────────────────────────────────
        var audienciasHoy = await _context.EventosProcesales
            .CountAsync(x => x.IsActive && x.RequiereAsistencia
                          && x.FechaHoraAgendada.HasValue
                          && x.FechaHoraAgendada.Value.Date == today,
                        cancellationToken);
        var audienciasProx7 = await _context.EventosProcesales
            .CountAsync(x => x.IsActive && x.RequiereAsistencia
                          && x.FechaHoraAgendada.HasValue
                          && x.FechaHoraAgendada.Value.Date >= today
                          && x.FechaHoraAgendada.Value.Date <= next7,
                        cancellationToken);

        // ── Comunicación ──────────────────────────────────────────
        var consultasSinLeer = await _context.ConsultasWeb.CountAsync(x => !x.Leida, cancellationToken);
        var notificacionesSinLeer = await _context.Notificaciones.CountAsync(x => !x.Leida, cancellationToken);
        var notificacionesHoy = await _context.Notificaciones
            .CountAsync(x => x.CreatedAt.Date == today, cancellationToken);

        // ── ✅ FASE 8: Métricas CRM por estado ────────────────────
        var consultasPendientes = await _context.ConsultasWeb
            .CountAsync(x => x.Estado == EstadoConsulta.Pendiente, cancellationToken);
        var consultasEnRevision = await _context.ConsultasWeb
            .CountAsync(x => x.Estado == EstadoConsulta.EnRevision, cancellationToken);
        var consultasCitas = await _context.ConsultasWeb
            .CountAsync(x => x.Estado == EstadoConsulta.CitaAgendada, cancellationToken);
        var consultasRespondidas = await _context.ConsultasWeb
            .CountAsync(x => x.Estado == EstadoConsulta.Respondida, cancellationToken);
        var consultasConcluidas = await _context.ConsultasWeb
            .CountAsync(x => x.Estado == EstadoConsulta.Concluida, cancellationToken);

        // ── Listas ────────────────────────────────────────────────
        var proximosPlazos = await _plazoService.GetUpcomingAsync(7, cancellationToken);

        var ultimasNotificaciones = await _context.Notificaciones
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new Application.Models.Notificaciones.NotificacionResponse
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Mensaje = x.Mensaje,
                Tipo = x.Tipo,
                RelacionTipo = x.RelacionTipo,
                RelacionId = x.RelacionId,
                FechaProgramada = x.FechaProgramada,
                FechaEnviada = x.FechaEnviada,
                Enviada = x.Enviada,
                Leida = x.Leida,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var proximasAudiencias = await _context.EventosProcesales
            .Include(x => x.Causa)
            .Where(x => x.IsActive && x.RequiereAsistencia
                     && x.FechaHoraAgendada.HasValue
                     && x.FechaHoraAgendada.Value >= DateTime.UtcNow)
            .OrderBy(x => x.FechaHoraAgendada)
            .Take(10)
            .ToListAsync(cancellationToken);

        var proximasAudienciasMapeadas = proximasAudiencias
            .Select(x => new Application.Models.Eventos.EventoProcesalResponse
            {
                Id = x.Id,
                CausaId = x.CausaId,
                NumeroProceso = x.Causa?.NumeroProceso ?? string.Empty,
                IdentificadorCausa = ObtenerIdentificador(x.Causa),
                Tipo = x.Tipo,
                Descripcion = x.Descripcion,
                FechaEvento = x.FechaEvento,
                EsAutomatico = x.EsAutomatico,
                Fuente = x.Fuente,
                RequiereAsistencia = x.RequiereAsistencia,
                FechaHoraAgendada = x.FechaHoraAgendada,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            }).ToList();

        return new DashboardSummaryResponse
        {
            TotalClientes = totalClientes,
            ClientesActivos = clientesActivos,
            TotalCausas = totalCausas,
            CausasActivas = causasActivas,
            TotalEventos = totalEventos,
            PlazosVencenHoy = plazosVencenHoy,
            PlazosProximos7Dias = plazosProximos7,
            AudienciasHoy = audienciasHoy,
            AudienciasProximos7Dias = audienciasProx7,
            ConsultasWebSinLeer = consultasSinLeer,
            NotificacionesSinLeer = notificacionesSinLeer,
            NotificacionesHoy = notificacionesHoy,

            // ✅ FASE 8
            ConsultasPendientes = consultasPendientes,
            ConsultasEnRevision = consultasEnRevision,
            ConsultasCitasAgendadas = consultasCitas,
            ConsultasRespondidas = consultasRespondidas,
            ConsultasConcluidas = consultasConcluidas,
            ConsultasTotalActivas = consultasPendientes + consultasEnRevision + consultasCitas,

            ProximosPlazos = proximosPlazos.Take(10).ToList(),
            UltimasNotificaciones = ultimasNotificaciones,
            ProximasAudiencias = proximasAudienciasMapeadas
        };
    }

    private static string ObtenerIdentificador(Domain.Entities.Causa? causa)
    {
        if (causa is null) return "sin causa";
        bool esPenal = causa.Materia?.Contains("penal", StringComparison.OrdinalIgnoreCase) == true;
        bool tieneProceso = !string.IsNullOrWhiteSpace(causa.NumeroProceso);
        bool tieneExpFiscal = !string.IsNullOrWhiteSpace(causa.ExpedienteFiscal);

        if (!esPenal) return tieneProceso ? causa.NumeroProceso! : "sin número";
        if (tieneProceso && tieneExpFiscal)
            return $"Proc: {causa.NumeroProceso} / Exp: {causa.ExpedienteFiscal}";
        if (tieneProceso) return causa.NumeroProceso!;
        if (tieneExpFiscal) return $"Fiscalía: {causa.ExpedienteFiscal}";
        return "sin identificador";
    }
}
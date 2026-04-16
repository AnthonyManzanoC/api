using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Notificaciones;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class NotificacionService : INotificacionService
{
    private readonly AppDbContext _context;

    public NotificacionService(AppDbContext context)
    {
        _context = context;
    }

    // ── Queries básicas ────────────────────────────────────────────

    public async Task<List<NotificacionResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notificaciones
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapToResponse())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificacionResponse>> GetUnreadAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notificaciones
            .Where(x => !x.Leida)
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapToResponse())
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificacionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notificaciones
            .Where(x => x.Id == id)
            .Select(MapToResponse())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<NotificacionResponse> CreateAsync(NotificacionCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Notificacion
        {
            Titulo = request.Titulo.Trim(),
            Mensaje = request.Mensaje.Trim(),
            Tipo = string.IsNullOrWhiteSpace(request.Tipo) ? "Sistema" : request.Tipo.Trim(),
            RelacionTipo = string.IsNullOrWhiteSpace(request.RelacionTipo) ? null : request.RelacionTipo.Trim(),
            RelacionId = request.RelacionId,
            FechaProgramada = request.FechaProgramada ?? DateTime.UtcNow,
            Enviada = false,
            Leida = false
        };

        _context.Notificaciones.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(entity);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Notificaciones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        entity.Leida = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Notificaciones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        _context.Notificaciones.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Motor de recordatorios: Plazos ────────────────────────────

    public async Task<int> GenerarRecordatoriosDePlazosAsync(int diasPorDelante = 3, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow.Date;
        var limite = hoy.AddDays(diasPorDelante);

        var plazos = await _context.Plazos
            .Include(x => x.EventoProcesal)
            .ThenInclude(x => x!.Causa)
            .Where(x => x.IsActive && !x.Cumplido
                     && x.FechaVencimiento.Date >= hoy
                     && x.FechaVencimiento.Date <= limite)
            .ToListAsync(cancellationToken);

        int creadas = 0;

        foreach (var plazo in plazos)
        {
            var yaExiste = await _context.Notificaciones.AnyAsync(n =>
                n.RelacionTipo == "Plazo" &&
                n.RelacionId == plazo.Id &&
                n.Tipo == "Recordatorio" &&
                n.IsActive, cancellationToken);

            if (yaExiste) continue;

            var diasRestantes = (plazo.FechaVencimiento.Date - hoy).Days;
            var causa = plazo.EventoProcesal?.Causa;
            var identificador = ObtenerIdentificadorCausa(causa);

            var titulo = diasRestantes == 0
                ? $"Plazo vence HOY: {plazo.Titulo}"
                : $"Plazo próximo ({diasRestantes}d): {plazo.Titulo}";

            var mensaje = diasRestantes == 0
                ? $"El plazo '{plazo.Titulo}' vence hoy — causa {identificador}."
                : $"El plazo '{plazo.Titulo}' vence en {diasRestantes} día(s) — causa {identificador}.";

            _context.Notificaciones.Add(new Notificacion
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = "Recordatorio",
                RelacionTipo = "Plazo",
                RelacionId = plazo.Id,
                FechaProgramada = DateTime.UtcNow,
                Enviada = true,
                FechaEnviada = DateTime.UtcNow,
                Leida = false
            });
            creadas++;
        }

        if (creadas > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return creadas;
    }

    public async Task<int> GenerarRecordatoriosVencidosAsync(CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow.Date;

        var plazos = await _context.Plazos
            .Include(x => x.EventoProcesal)
            .ThenInclude(x => x!.Causa)
            .Where(x => x.IsActive && !x.Cumplido && x.FechaVencimiento.Date < hoy)
            .ToListAsync(cancellationToken);

        int creadas = 0;

        foreach (var plazo in plazos)
        {
            var yaExiste = await _context.Notificaciones.AnyAsync(n =>
                n.RelacionTipo == "Plazo" &&
                n.RelacionId == plazo.Id &&
                n.Tipo == "Vencido" &&
                n.IsActive, cancellationToken);

            if (yaExiste) continue;

            var causa = plazo.EventoProcesal?.Causa;
            var identificador = ObtenerIdentificadorCausa(causa);

            _context.Notificaciones.Add(new Notificacion
            {
                Titulo = $"⚠ Plazo vencido: {plazo.Titulo}",
                Mensaje = $"El plazo '{plazo.Titulo}' ya expiró — causa {identificador}. Requiere atención.",
                Tipo = "Vencido",
                RelacionTipo = "Plazo",
                RelacionId = plazo.Id,
                FechaProgramada = DateTime.UtcNow,
                Enviada = true,
                FechaEnviada = DateTime.UtcNow,
                Leida = false
            });
            creadas++;
        }

        if (creadas > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return creadas;
    }

    // ── Motor de recordatorios: Audiencias ✅ NUEVO ───────────────

    public async Task<int> GenerarRecordatoriosDeAudienciasAsync(int diasPorDelante = 2, CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.UtcNow;
        var limite = ahora.Date.AddDays(diasPorDelante).AddDays(1); // hasta fin del día límite

        // Buscar eventos que requieren asistencia y tienen hora agendada en los próximos N días
        var eventos = await _context.EventosProcesales
            .Include(x => x.Causa)
            .Where(x => x.IsActive
                     && x.RequiereAsistencia
                     && x.FechaHoraAgendada.HasValue
                     && x.FechaHoraAgendada.Value >= ahora
                     && x.FechaHoraAgendada.Value <= limite)
            .ToListAsync(cancellationToken);

        int creadas = 0;

        foreach (var evento in eventos)
        {
            // Una sola notificación por evento (no duplicar)
            var yaExiste = await _context.Notificaciones.AnyAsync(n =>
                n.RelacionTipo == "Evento" &&
                n.RelacionId == evento.Id &&
                n.Tipo == "Audiencia" &&
                n.IsActive, cancellationToken);

            if (yaExiste) continue;

            var identificador = ObtenerIdentificadorCausa(evento.Causa);
            var fechaHora = evento.FechaHoraAgendada!.Value;
            var horaTexto = fechaHora.ToLocalTime().ToString("dd/MM/yyyy 'a las' HH:mm");
            var diasFaltan = (fechaHora.Date - ahora.Date).Days;

            var titulo = diasFaltan == 0
                ? $"🏛 HOYA — {evento.Tipo}: {evento.Descripcion[..Math.Min(50, evento.Descripcion.Length)]}"
                : $"🏛 Audiencia en {diasFaltan}d — {evento.Tipo}";

            var mensaje = $"Diligencia '{evento.Tipo}' programada el {horaTexto} — causa {identificador}.";

            _context.Notificaciones.Add(new Notificacion
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = "Audiencia",
                RelacionTipo = "Evento",
                RelacionId = evento.Id,
                FechaProgramada = fechaHora,
                Enviada = true,
                FechaEnviada = DateTime.UtcNow,
                Leida = false
            });
            creadas++;
        }

        if (creadas > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return creadas;
    }

    // ── Helpers privados ──────────────────────────────────────────

    /// <summary>
    /// Devuelve el identificador visual de una causa (número proceso o expediente fiscal).
    /// </summary>
    private static string ObtenerIdentificadorCausa(Domain.Entities.Causa? causa)
    {
        if (causa is null) return "sin causa";

        bool esPenal = causa.Materia?.Contains("penal", StringComparison.OrdinalIgnoreCase) == true;
        bool tieneProceso = !string.IsNullOrWhiteSpace(causa.NumeroProceso);
        bool tieneExpFiscal = !string.IsNullOrWhiteSpace(causa.ExpedienteFiscal);

        if (!esPenal) return tieneProceso ? causa.NumeroProceso! : "sin número";

        if (tieneProceso && tieneExpFiscal) return $"Proc: {causa.NumeroProceso} / Exp: {causa.ExpedienteFiscal}";
        if (tieneProceso) return $"Proceso {causa.NumeroProceso}";
        if (tieneExpFiscal) return $"Fiscalía: {causa.ExpedienteFiscal}";

        return "sin identificador";
    }

    private static System.Linq.Expressions.Expression<Func<Notificacion, NotificacionResponse>> MapToResponse()
        => x => new NotificacionResponse
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
        };

    private static NotificacionResponse ToResponse(Notificacion x) => new()
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
    };
}

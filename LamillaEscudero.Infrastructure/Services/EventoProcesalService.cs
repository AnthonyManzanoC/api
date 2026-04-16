using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Eventos;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class EventoProcesalService : IEventoProcesalService
{
    private readonly AppDbContext _context;

    public EventoProcesalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventoProcesalResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var eventos = await _context.EventosProcesales
            .Include(x => x.Causa)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return eventos.Select(MapToResponse).ToList();
    }

    public async Task<List<EventoProcesalResponse>> GetByCausaIdAsync(Guid causaId, CancellationToken cancellationToken = default)
    {
        var eventos = await _context.EventosProcesales
            .Include(x => x.Causa)
            .Where(x => x.CausaId == causaId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return eventos.Select(MapToResponse).ToList();
    }

    public async Task<EventoProcesalResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evento = await _context.EventosProcesales
            .Include(x => x.Causa)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return evento is null ? null : MapToResponse(evento);
    }

    public async Task<EventoProcesalResponse> CreateAsync(EventoProcesalCreateRequest request, CancellationToken cancellationToken = default)
    {
        var causa = await _context.Causas.FirstOrDefaultAsync(x => x.Id == request.CausaId, cancellationToken);
        if (causa is null)
            throw new InvalidOperationException("La causa no existe.");

        var entity = new EventoProcesal
        {
            CausaId = request.CausaId,
            Tipo = request.Tipo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            FechaEvento = request.FechaEvento,
            EsAutomatico = request.EsAutomatico,
            Fuente = string.IsNullOrWhiteSpace(request.Fuente) ? null : request.Fuente.Trim(),
            RequiereAsistencia = request.RequiereAsistencia,
            FechaHoraAgendada = request.RequiereAsistencia ? request.FechaHoraAgendada : null
        };

        _context.EventosProcesales.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        entity.Causa = causa;
        return MapToResponse(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, EventoProcesalUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.EventosProcesales.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        var causaExiste = await _context.Causas.AnyAsync(x => x.Id == request.CausaId, cancellationToken);
        if (!causaExiste)
            throw new InvalidOperationException("La causa no existe.");

        entity.CausaId = request.CausaId;
        entity.Tipo = request.Tipo.Trim();
        entity.Descripcion = request.Descripcion.Trim();
        entity.FechaEvento = request.FechaEvento;
        entity.EsAutomatico = request.EsAutomatico;
        entity.Fuente = string.IsNullOrWhiteSpace(request.Fuente) ? null : request.Fuente.Trim();
        entity.RequiereAsistencia = request.RequiereAsistencia;
        entity.FechaHoraAgendada = request.RequiereAsistencia ? request.FechaHoraAgendada : null;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.EventosProcesales.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        _context.EventosProcesales.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Mapeador con lógica de identificador de causa ─────────────
    private static EventoProcesalResponse MapToResponse(EventoProcesal x)
    {
        var causa = x.Causa;
        string identificador;
        string numeroProceso;

        if (causa is null)
        {
            identificador = "sin causa";
            numeroProceso = string.Empty;
        }
        else
        {
            numeroProceso = causa.NumeroProceso ?? string.Empty;
            bool esPenal = causa.Materia?.Contains("penal", StringComparison.OrdinalIgnoreCase) == true;
            bool tieneProceso = !string.IsNullOrWhiteSpace(causa.NumeroProceso);
            bool tieneExpFiscal = !string.IsNullOrWhiteSpace(causa.ExpedienteFiscal);

            if (!esPenal) identificador = tieneProceso ? causa.NumeroProceso! : "sin número";
            else if (tieneProceso && tieneExpFiscal) identificador = $"Proc: {causa.NumeroProceso} / Exp: {causa.ExpedienteFiscal}";
            else if (tieneProceso) identificador = causa.NumeroProceso!;
            else if (tieneExpFiscal) identificador = $"Fiscalía: {causa.ExpedienteFiscal}";
            else identificador = "sin identificador";
        }

        return new EventoProcesalResponse
        {
            Id = x.Id,
            CausaId = x.CausaId,
            NumeroProceso = numeroProceso,
            IdentificadorCausa = identificador,
            Tipo = x.Tipo,
            Descripcion = x.Descripcion,
            FechaEvento = x.FechaEvento,
            EsAutomatico = x.EsAutomatico,
            Fuente = x.Fuente,
            RequiereAsistencia = x.RequiereAsistencia,
            FechaHoraAgendada = x.FechaHoraAgendada,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        };
    }
}

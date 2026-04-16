using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Plazos;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class PlazoService : IPlazoService
{
    private readonly AppDbContext _context;

    public PlazoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlazoResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plazos
            .Include(x => x.EventoProcesal)
            .ThenInclude(x => x!.Causa)
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new PlazoResponse
            {
                Id = x.Id,
                EventoProcesalId = x.EventoProcesalId,
                CausaId = x.EventoProcesal != null ? x.EventoProcesal.CausaId : Guid.Empty,
                NumeroProceso = x.EventoProcesal != null && x.EventoProcesal.Causa != null ? x.EventoProcesal.Causa.NumeroProceso : string.Empty,
                Titulo = x.Titulo,
                FechaVencimiento = x.FechaVencimiento,
                ConfirmadoPorAbogado = x.ConfirmadoPorAbogado,
                Cumplido = x.Cumplido,
                Notas = x.Notas,
                TipoEvento = x.EventoProcesal != null ? x.EventoProcesal.Tipo : null,
                FechaEvento = x.EventoProcesal != null ? x.EventoProcesal.FechaEvento : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlazoResponse>> GetUpcomingAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(days);

        return await _context.Plazos
            .Include(x => x.EventoProcesal)
            .ThenInclude(x => x!.Causa)
            .Where(x => !x.Cumplido && x.FechaVencimiento >= from && x.FechaVencimiento <= to)
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new PlazoResponse
            {
                Id = x.Id,
                EventoProcesalId = x.EventoProcesalId,
                CausaId = x.EventoProcesal != null ? x.EventoProcesal.CausaId : Guid.Empty,
                NumeroProceso = x.EventoProcesal != null && x.EventoProcesal.Causa != null ? x.EventoProcesal.Causa.NumeroProceso : string.Empty,
                Titulo = x.Titulo,
                FechaVencimiento = x.FechaVencimiento,
                ConfirmadoPorAbogado = x.ConfirmadoPorAbogado,
                Cumplido = x.Cumplido,
                Notas = x.Notas,
                TipoEvento = x.EventoProcesal != null ? x.EventoProcesal.Tipo : null,
                FechaEvento = x.EventoProcesal != null ? x.EventoProcesal.FechaEvento : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlazoResponse>> GetByEventoProcesalIdAsync(Guid eventoProcesalId, CancellationToken cancellationToken = default)
    {
        return await _context.Plazos
            .Include(x => x.EventoProcesal)
            .ThenInclude(x => x!.Causa)
            .Where(x => x.EventoProcesalId == eventoProcesalId)
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new PlazoResponse
            {
                Id = x.Id,
                EventoProcesalId = x.EventoProcesalId,
                CausaId = x.EventoProcesal != null ? x.EventoProcesal.CausaId : Guid.Empty,
                NumeroProceso = x.EventoProcesal != null && x.EventoProcesal.Causa != null ? x.EventoProcesal.Causa.NumeroProceso : string.Empty,
                Titulo = x.Titulo,
                FechaVencimiento = x.FechaVencimiento,
                ConfirmadoPorAbogado = x.ConfirmadoPorAbogado,
                Cumplido = x.Cumplido,
                Notas = x.Notas,
                TipoEvento = x.EventoProcesal != null ? x.EventoProcesal.Tipo : null,
                FechaEvento = x.EventoProcesal != null ? x.EventoProcesal.FechaEvento : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PlazoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plazos
            .Include(x => x.EventoProcesal)
            .ThenInclude(x => x!.Causa)
            .Where(x => x.Id == id)
            .Select(x => new PlazoResponse
            {
                Id = x.Id,
                EventoProcesalId = x.EventoProcesalId,
                CausaId = x.EventoProcesal != null ? x.EventoProcesal.CausaId : Guid.Empty,
                NumeroProceso = x.EventoProcesal != null && x.EventoProcesal.Causa != null ? x.EventoProcesal.Causa.NumeroProceso : string.Empty,
                Titulo = x.Titulo,
                FechaVencimiento = x.FechaVencimiento,
                ConfirmadoPorAbogado = x.ConfirmadoPorAbogado,
                Cumplido = x.Cumplido,
                Notas = x.Notas,
                TipoEvento = x.EventoProcesal != null ? x.EventoProcesal.Tipo : null,
                FechaEvento = x.EventoProcesal != null ? x.EventoProcesal.FechaEvento : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PlazoResponse> CreateAsync(PlazoCreateRequest request, CancellationToken cancellationToken = default)
    {
        var evento = await _context.EventosProcesales
            .Include(x => x.Causa)
            .FirstOrDefaultAsync(x => x.Id == request.EventoProcesalId, cancellationToken);

        if (evento is null)
            throw new InvalidOperationException("El evento procesal no existe.");

        var entity = new Plazo
        {
            EventoProcesalId = request.EventoProcesalId,
            Titulo = request.Titulo.Trim(),
            FechaVencimiento = request.FechaVencimiento,
            ConfirmadoPorAbogado = request.ConfirmadoPorAbogado,
            Cumplido = request.Cumplido,
            Notas = string.IsNullOrWhiteSpace(request.Notas) ? null : request.Notas.Trim()
        };

        _context.Plazos.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new PlazoResponse
        {
            Id = entity.Id,
            EventoProcesalId = entity.EventoProcesalId,
            CausaId = evento.CausaId,
            NumeroProceso = evento.Causa?.NumeroProceso ?? string.Empty,
            Titulo = entity.Titulo,
            FechaVencimiento = entity.FechaVencimiento,
            ConfirmadoPorAbogado = entity.ConfirmadoPorAbogado,
            Cumplido = entity.Cumplido,
            Notas = entity.Notas,
            TipoEvento = evento.Tipo,
            FechaEvento = evento.FechaEvento,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(Guid id, PlazoUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Plazos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        var eventoExiste = await _context.EventosProcesales.AnyAsync(x => x.Id == request.EventoProcesalId, cancellationToken);
        if (!eventoExiste)
            throw new InvalidOperationException("El evento procesal no existe.");

        entity.EventoProcesalId = request.EventoProcesalId;
        entity.Titulo = request.Titulo.Trim();
        entity.FechaVencimiento = request.FechaVencimiento;
        entity.ConfirmadoPorAbogado = request.ConfirmadoPorAbogado;
        entity.Cumplido = request.Cumplido;
        entity.Notas = string.IsNullOrWhiteSpace(request.Notas) ? null : request.Notas.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkCumplidoAsync(Guid id, bool cumplido, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Plazos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        entity.Cumplido = cumplido;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Plazos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        _context.Plazos.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Servicios;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class ServicioOfrecidoService : IServicioOfrecidoService
{
    private readonly AppDbContext _ctx;
    public ServicioOfrecidoService(AppDbContext ctx) => _ctx = ctx;

    public async Task<List<ServicioOfrecidoResponse>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.ServiciosOfrecidos
            .Where(x => x.IsActive)
            .OrderBy(x => x.Orden)
            .Select(x => new ServicioOfrecidoResponse
            {
                Id = x.Id,
                Titulo = x.Titulo,
                DescripcionCorta = x.DescripcionCorta,
                Detalles = x.Detalles,
                Icono = x.Icono,
                Orden = x.Orden,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

    public async Task<ServicioOfrecidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.ServiciosOfrecidos.Where(x => x.Id == id)
            .Select(x => new ServicioOfrecidoResponse
            {
                Id = x.Id,
                Titulo = x.Titulo,
                DescripcionCorta = x.DescripcionCorta,
                Detalles = x.Detalles,
                Icono = x.Icono,
                Orden = x.Orden,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            }).FirstOrDefaultAsync(ct);

    public async Task<ServicioOfrecidoResponse> CreateAsync(ServicioOfrecidoCreateRequest r, CancellationToken ct = default)
    {
        var e = new ServicioOfrecido
        {
            Titulo = r.Titulo.Trim(),
            DescripcionCorta = r.DescripcionCorta.Trim(),
            Detalles = r.Detalles?.Trim(),
            Icono = r.Icono?.Trim() ?? "bi-briefcase",
            Orden = r.Orden
        };
        _ctx.ServiciosOfrecidos.Add(e);
        await _ctx.SaveChangesAsync(ct);
        return new ServicioOfrecidoResponse
        {
            Id = e.Id,
            Titulo = e.Titulo,
            DescripcionCorta = e.DescripcionCorta,
            Detalles = e.Detalles,
            Icono = e.Icono,
            Orden = e.Orden,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(Guid id, ServicioOfrecidoUpdateRequest r, CancellationToken ct = default)
    {
        var e = await _ctx.ServiciosOfrecidos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        e.Titulo = r.Titulo.Trim(); e.DescripcionCorta = r.DescripcionCorta.Trim();
        e.Detalles = r.Detalles?.Trim(); e.Icono = r.Icono?.Trim();
        e.Orden = r.Orden; e.IsActive = r.IsActive; e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _ctx.ServiciosOfrecidos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _ctx.ServiciosOfrecidos.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}
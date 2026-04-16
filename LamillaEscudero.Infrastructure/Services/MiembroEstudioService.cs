// ══════════════════════════════════════════════════════════════════
//  MiembroEstudioService.cs  —  Fase 9
// ══════════════════════════════════════════════════════════════════
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Miembros;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class MiembroEstudioService : IMiembroEstudioService
{
    private readonly AppDbContext _ctx;
    public MiembroEstudioService(AppDbContext ctx) => _ctx = ctx;

    // ── Queries ───────────────────────────────────────────────

    public async Task<List<MiembroEstudioResponse>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.MiembrosEstudio
            .Where(x => x.IsActive)
            .OrderBy(x => x.Orden)
            .Select(MapExpr())
            .ToListAsync(ct);

    public async Task<MiembroEstudioResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.MiembrosEstudio
            .Where(x => x.Id == id)
            .Select(MapExpr())
            .FirstOrDefaultAsync(ct);

    // ── Mutations ─────────────────────────────────────────────

    public async Task<MiembroEstudioResponse> CreateAsync(
        MiembroEstudioCreateRequest r, CancellationToken ct = default)
    {
        var e = BuildEntity(r);
        _ctx.MiembrosEstudio.Add(e);
        await _ctx.SaveChangesAsync(ct);
        return ToResponse(e);
    }

    public async Task<bool> UpdateAsync(
        Guid id, MiembroEstudioUpdateRequest r, CancellationToken ct = default)
    {
        var e = await _ctx.MiembrosEstudio.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        e.Nombres = r.Nombres.Trim();
        e.Cargo = r.Cargo.Trim();
        e.BiografiaBreve = r.BiografiaBreve?.Trim();
        e.BiografiaCompleta = r.BiografiaCompleta?.Trim();

        // foto — si llega PhotoData nuevo, lo aplica; si llega vacío, conserva o limpia según FotoUrl
        e.PhotoData = string.IsNullOrWhiteSpace(r.PhotoData) ? null : r.PhotoData;
        e.FotoUrl = string.IsNullOrWhiteSpace(r.FotoUrl) ? null : r.FotoUrl.Trim();

        // perfil extendido
        e.FraseDestacada = r.FraseDestacada?.Trim();
        e.BiografiaLarga = r.BiografiaLarga?.Trim();
        e.EducacionJson = string.IsNullOrWhiteSpace(r.EducacionJson) ? null : r.EducacionJson;
        e.TimelineExperienciaJson = string.IsNullOrWhiteSpace(r.TimelineExperienciaJson) ? null : r.TimelineExperienciaJson;

        e.LinkedIn = r.LinkedIn?.Trim();
        e.Email = r.Email?.Trim();
        e.Orden = r.Orden;
        e.IsActive = r.IsActive;
        e.UpdatedAt = DateTime.UtcNow;

        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _ctx.MiembrosEstudio.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _ctx.MiembrosEstudio.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    // ── Private helpers ───────────────────────────────────────

    private static MiembroEstudio BuildEntity(MiembroEstudioCreateRequest r) => new()
    {
        Nombres = r.Nombres.Trim(),
        Cargo = r.Cargo.Trim(),
        BiografiaBreve = r.BiografiaBreve?.Trim(),
        BiografiaCompleta = r.BiografiaCompleta?.Trim(),
        FotoUrl = string.IsNullOrWhiteSpace(r.FotoUrl) ? null : r.FotoUrl.Trim(),
        PhotoData = string.IsNullOrWhiteSpace(r.PhotoData) ? null : r.PhotoData,
        FraseDestacada = r.FraseDestacada?.Trim(),
        BiografiaLarga = r.BiografiaLarga?.Trim(),
        EducacionJson = string.IsNullOrWhiteSpace(r.EducacionJson) ? null : r.EducacionJson,
        TimelineExperienciaJson = string.IsNullOrWhiteSpace(r.TimelineExperienciaJson) ? null : r.TimelineExperienciaJson,
        LinkedIn = r.LinkedIn?.Trim(),
        Email = r.Email?.Trim(),
        Orden = r.Orden
    };

    private static System.Linq.Expressions.Expression<Func<MiembroEstudio, MiembroEstudioResponse>>
        MapExpr() => x => new MiembroEstudioResponse
        {
            Id = x.Id,
            Nombres = x.Nombres,
            Cargo = x.Cargo,
            BiografiaBreve = x.BiografiaBreve,
            BiografiaCompleta = x.BiografiaCompleta,
            FotoUrl = x.FotoUrl,
            PhotoData = x.PhotoData,
            FraseDestacada = x.FraseDestacada,
            BiografiaLarga = x.BiografiaLarga,
            EducacionJson = x.EducacionJson,
            TimelineExperienciaJson = x.TimelineExperienciaJson,
            LinkedIn = x.LinkedIn,
            Email = x.Email,
            Orden = x.Orden,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        };

    private static MiembroEstudioResponse ToResponse(MiembroEstudio x) => new()
    {
        Id = x.Id,
        Nombres = x.Nombres,
        Cargo = x.Cargo,
        BiografiaBreve = x.BiografiaBreve,
        BiografiaCompleta = x.BiografiaCompleta,
        FotoUrl = x.FotoUrl,
        PhotoData = x.PhotoData,
        FraseDestacada = x.FraseDestacada,
        BiografiaLarga = x.BiografiaLarga,
        EducacionJson = x.EducacionJson,
        TimelineExperienciaJson = x.TimelineExperienciaJson,
        LinkedIn = x.LinkedIn,
        Email = x.Email,
        Orden = x.Orden,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt
    };
}
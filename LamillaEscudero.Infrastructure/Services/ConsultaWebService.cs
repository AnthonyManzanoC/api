using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Consultas;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class ConsultaWebService : IConsultaWebService
{
    private readonly AppDbContext _context;

    public ConsultaWebService(AppDbContext context) => _context = context;

    // ── Queries ───────────────────────────────────────────────────

    public async Task<List<ConsultaWebResponse>> GetAllAsync(CancellationToken ct = default)
        => await _context.ConsultasWeb
            .OrderByDescending(x => x.CreatedAt)
            .Select(Map())
            .ToListAsync(ct);

    public async Task<List<ConsultaWebResponse>> GetUnReadAsync(CancellationToken ct = default)
        => await _context.ConsultasWeb
            .Where(x => !x.Leida)
            .OrderByDescending(x => x.CreatedAt)
            .Select(Map())
            .ToListAsync(ct);

    public async Task<ConsultaWebResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ConsultasWeb
            .Where(x => x.Id == id)
            .Select(Map())
            .FirstOrDefaultAsync(ct);

    // ── Mutaciones ────────────────────────────────────────────────

    public async Task<ConsultaWebResponse> CreateAsync(ConsultaWebCreateRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Asunto))
            throw new InvalidOperationException("El asunto es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Mensaje))
            throw new InvalidOperationException("El mensaje es obligatorio.");

        var entity = new ConsultaWeb
        {
            Nombre = request.Nombre.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? null : request.Telefono.Trim(),
            Asunto = request.Asunto.Trim(),
            Mensaje = request.Mensaje.Trim(),
            Leida = false,
            Estado = EstadoConsulta.Pendiente
        };

        _context.ConsultasWeb.Add(entity);
        await _context.SaveChangesAsync(ct);

        return ToResponse(entity);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.ConsultasWeb.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.Leida = true;
        entity.UpdatedAt = DateTime.UtcNow;

        // Al leer, si aún estaba Pendiente, pasa a En Revisión
        if (entity.Estado == EstadoConsulta.Pendiente)
            entity.Estado = EstadoConsulta.EnRevision;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, ConsultaWebUpdateRequest request, CancellationToken ct = default)
    {
        var entity = await _context.ConsultasWeb.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.Estado = request.Estado;
        entity.FechaCita = request.FechaCita;
        entity.NotasInternas = string.IsNullOrWhiteSpace(request.NotasInternas)
                                ? null : request.NotasInternas.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        // Marcar leída si se actualiza el CRM
        if (!entity.Leida) entity.Leida = true;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.ConsultasWeb.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _context.ConsultasWeb.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static System.Linq.Expressions.Expression<Func<ConsultaWeb, ConsultaWebResponse>> Map()
        => x => new ConsultaWebResponse
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Email = x.Email,
            Telefono = x.Telefono,
            Asunto = x.Asunto,
            Mensaje = x.Mensaje,
            Leida = x.Leida,
            Estado = x.Estado,
            FechaCita = x.FechaCita,
            NotasInternas = x.NotasInternas,
            CreatedAt = x.CreatedAt
        };

    private static ConsultaWebResponse ToResponse(ConsultaWeb x) => new()
    {
        Id = x.Id,
        Nombre = x.Nombre,
        Email = x.Email,
        Telefono = x.Telefono,
        Asunto = x.Asunto,
        Mensaje = x.Mensaje,
        Leida = x.Leida,
        Estado = x.Estado,
        FechaCita = x.FechaCita,
        NotasInternas = x.NotasInternas,
        CreatedAt = x.CreatedAt
    };
}

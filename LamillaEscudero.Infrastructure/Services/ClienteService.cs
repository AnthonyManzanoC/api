using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Abonos;
using LamillaEscudero.Application.Models.Clientes;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly AppDbContext _context;
    public ClienteService(AppDbContext context) => _context = context;

    // ── Queries ───────────────────────────────────────────────

    public async Task<List<ClienteResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var clientes = await _context.Clientes
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        // Suma de abonos por cliente en una sola query
        var clienteIds = clientes.Select(c => c.Id).ToList();
        var abonados = await _context.Abonos
            .Where(a => clienteIds.Contains(a.ClienteId))
            .GroupBy(a => a.ClienteId)
            .Select(g => new { ClienteId = g.Key, Total = g.Sum(a => a.Monto) })
            .ToListAsync(ct);

        var abonadoDict = abonados.ToDictionary(x => x.ClienteId, x => x.Total);

        return clientes.Select(x => ToResponse(x, abonadoDict.GetValueOrDefault(x.Id, 0m))).ToList();
    }

    public async Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return null;

        var totalAbonado = await _context.Abonos
            .Where(a => a.ClienteId == id)
            .SumAsync(a => a.Monto, ct);

        return ToResponse(entity, totalAbonado);
    }

    public async Task<ClienteResponse> CreateAsync(ClienteCreateRequest request, CancellationToken ct = default)
    {
        var entity = new Cliente
        {
            Nombres = request.Nombres.Trim(),
            Cedula = NullIfEmpty(request.Cedula),
            Email = NullIfEmpty(request.Email),
            Telefono = NullIfEmpty(request.Telefono),
            Direccion = NullIfEmpty(request.Direccion),
            Observaciones = NullIfEmpty(request.Observaciones)
        };
        _context.Clientes.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToResponse(entity, 0m);
    }

    public async Task<bool> UpdateAsync(Guid id, ClienteUpdateRequest request, CancellationToken ct = default)
    {
        var entity = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.Nombres = request.Nombres.Trim();
        entity.Cedula = NullIfEmpty(request.Cedula);
        entity.Email = NullIfEmpty(request.Email);
        entity.Telefono = NullIfEmpty(request.Telefono);
        entity.Direccion = NullIfEmpty(request.Direccion);
        entity.Observaciones = NullIfEmpty(request.Observaciones);
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _context.Clientes.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ✅ FASE 10 — Honorarios ──────────────────────────────────

    public async Task<bool> UpdateHonorariosAsync(
        Guid clienteId, decimal totalHonorarios, CancellationToken ct = default)
    {
        var entity = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == clienteId, ct);
        if (entity is null) return false;

        entity.TotalHonorarios = totalHonorarios;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ✅ FASE 10 — Abonos ──────────────────────────────────────

    public async Task<AbonoResponse> AddAbonoAsync(
        AbonoCreateRequest request, CancellationToken ct = default)
    {
        var clienteExiste = await _context.Clientes.AnyAsync(x => x.Id == request.ClienteId, ct);
        if (!clienteExiste)
            throw new InvalidOperationException("El cliente no existe.");

        var entity = new Abono
        {
            ClienteId = request.ClienteId,
            Monto = request.Monto,
            FechaAbono = request.FechaAbono.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(request.FechaAbono, DateTimeKind.Utc)
                            : request.FechaAbono.ToUniversalTime(),
            Observacion = NullIfEmpty(request.Observacion)
        };

        _context.Abonos.Add(entity);
        await _context.SaveChangesAsync(ct);
        return MapAbono(entity);
    }

    public async Task<List<AbonoResponse>> GetAbonosByClienteAsync(
        Guid clienteId, CancellationToken ct = default)
    {
        return await _context.Abonos
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.FechaAbono)
            .Select(a => new AbonoResponse
            {
                Id = a.Id,
                ClienteId = a.ClienteId,
                Monto = a.Monto,
                FechaAbono = a.FechaAbono,
                Observacion = a.Observacion,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteAbonoAsync(Guid abonoId, CancellationToken ct = default)
    {
        var entity = await _context.Abonos.FirstOrDefaultAsync(x => x.Id == abonoId, ct);
        if (entity is null) return false;

        _context.Abonos.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────

    private static string? NullIfEmpty(string? v)
        => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static ClienteResponse ToResponse(Cliente x, decimal totalAbonado) => new()
    {
        Id = x.Id,
        Nombres = x.Nombres,
        Cedula = x.Cedula,
        Email = x.Email,
        Telefono = x.Telefono,
        Direccion = x.Direccion,
        Observaciones = x.Observaciones,
        UserId = x.UserId,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt,
        TotalHonorarios = x.TotalHonorarios,
        TotalAbonado = totalAbonado
    };

    private static AbonoResponse MapAbono(Abono a) => new()
    {
        Id = a.Id,
        ClienteId = a.ClienteId,
        Monto = a.Monto,
        FechaAbono = a.FechaAbono,
        Observacion = a.Observacion,
        CreatedAt = a.CreatedAt
    };
}
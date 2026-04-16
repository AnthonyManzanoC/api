using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Causas;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class CausaService : ICausaService
{
    private readonly AppDbContext _context;

    public CausaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CausaResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Causas
            .Include(x => x.Cliente)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CausaResponse>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Causas
            .Include(x => x.Cliente)
            .Where(x => x.ClienteId == clienteId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<CausaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var causa = await _context.Causas
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return causa is null ? null : MapToResponse(causa);
    }

    public async Task<CausaResponse> CreateAsync(CausaCreateRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == request.ClienteId, cancellationToken);
        if (cliente is null)
            throw new InvalidOperationException("El cliente no existe.");

        // Validación: causas no penales deben tener número de proceso
        bool esPenal = request.Materia?.Contains("penal", StringComparison.OrdinalIgnoreCase) == true;
        if (!esPenal && string.IsNullOrWhiteSpace(request.NumeroProceso))
            throw new InvalidOperationException("El número de proceso es obligatorio para causas no penales.");

        // Al menos uno de los dos identificadores debe existir
        if (string.IsNullOrWhiteSpace(request.NumeroProceso) && string.IsNullOrWhiteSpace(request.ExpedienteFiscal))
            throw new InvalidOperationException("Debe ingresar al menos el Número de Proceso o el Expediente Fiscal.");

        var entity = new Causa
        {
            ClienteId = request.ClienteId,
            NumeroProceso = string.IsNullOrWhiteSpace(request.NumeroProceso) ? null : request.NumeroProceso.Trim(),
            ExpedienteFiscal = string.IsNullOrWhiteSpace(request.ExpedienteFiscal) ? null : request.ExpedienteFiscal.Trim(),
            UnidadJudicial = string.IsNullOrWhiteSpace(request.UnidadJudicial) ? null : request.UnidadJudicial.Trim(),
            Materia = string.IsNullOrWhiteSpace(request.Materia) ? null : request.Materia.Trim(),
            Estado = string.IsNullOrWhiteSpace(request.Estado) ? null : request.Estado.Trim(),
            FechaIngreso = request.FechaIngreso,
            Resumen = string.IsNullOrWhiteSpace(request.Resumen) ? null : request.Resumen.Trim()
        };

        _context.Causas.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        entity.Cliente = cliente;
        return MapToResponse(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, CausaUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Causas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        var clienteExiste = await _context.Clientes.AnyAsync(x => x.Id == request.ClienteId, cancellationToken);
        if (!clienteExiste)
            throw new InvalidOperationException("El cliente no existe.");

        entity.ClienteId = request.ClienteId;
        entity.NumeroProceso = string.IsNullOrWhiteSpace(request.NumeroProceso) ? null : request.NumeroProceso.Trim();
        entity.ExpedienteFiscal = string.IsNullOrWhiteSpace(request.ExpedienteFiscal) ? null : request.ExpedienteFiscal.Trim();
        entity.UnidadJudicial = string.IsNullOrWhiteSpace(request.UnidadJudicial) ? null : request.UnidadJudicial.Trim();
        entity.Materia = string.IsNullOrWhiteSpace(request.Materia) ? null : request.Materia.Trim();
        entity.Estado = string.IsNullOrWhiteSpace(request.Estado) ? null : request.Estado.Trim();
        entity.FechaIngreso = request.FechaIngreso;
        entity.Resumen = string.IsNullOrWhiteSpace(request.Resumen) ? null : request.Resumen.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Causas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        _context.Causas.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Mapeador centralizado ─────────────────────────────────────
    private static CausaResponse MapToResponse(Causa x) => new()
    {
        Id = x.Id,
        ClienteId = x.ClienteId,
        ClienteNombre = x.Cliente?.Nombres ?? string.Empty,
        NumeroProceso = x.NumeroProceso,
        ExpedienteFiscal = x.ExpedienteFiscal,
        UnidadJudicial = x.UnidadJudicial,
        Materia = x.Materia,
        Estado = x.Estado,
        FechaIngreso = x.FechaIngreso,
        Resumen = x.Resumen,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt
    };
}

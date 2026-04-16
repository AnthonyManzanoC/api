using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Documentos;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class DocumentoService : IDocumentoService
{
    private readonly AppDbContext _context;

    public DocumentoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentoResponse>> GetByCausaIdAsync(Guid causaId, CancellationToken cancellationToken = default)
    {
        return await _context.Documentos
            .Include(x => x.Causa)
            .Where(x => x.CausaId == causaId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentoResponse
            {
                Id = x.Id,
                CausaId = x.CausaId,
                NumeroProceso = x.Causa != null ? x.Causa.NumeroProceso : string.Empty,
                Nombre = x.Nombre,
                RutaArchivo = x.RutaArchivo,
                TipoMime = x.TipoMime,
                TamanoBytes = x.TamanoBytes,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documentos
            .Include(x => x.Causa)
            .Where(x => x.Id == id)
            .Select(x => new DocumentoResponse
            {
                Id = x.Id,
                CausaId = x.CausaId,
                NumeroProceso = x.Causa != null ? x.Causa.NumeroProceso : string.Empty,
                Nombre = x.Nombre,
                RutaArchivo = x.RutaArchivo,
                TipoMime = x.TipoMime,
                TamanoBytes = x.TamanoBytes,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DocumentoResponse> UploadAsync(Guid causaId, string nombre, Stream archivo, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var causa = await _context.Causas.FirstOrDefaultAsync(x => x.Id == causaId, cancellationToken);
        if (causa is null)
            throw new InvalidOperationException("La causa no existe.");

        // Corrección: Usamos Directory.GetCurrentDirectory() para no requerir librerías exclusivas de Web
        var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "causas", causaId.ToString());
        Directory.CreateDirectory(root);

        var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(root, safeName);

        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await archivo.CopyToAsync(fs, cancellationToken);
        }

        var entity = new Documento
        {
            CausaId = causaId,
            Nombre = string.IsNullOrWhiteSpace(nombre) ? Path.GetFileNameWithoutExtension(fileName) : nombre.Trim(),
            RutaArchivo = $"/uploads/causas/{causaId}/{safeName}",
            TipoMime = contentType,
            TamanoBytes = new FileInfo(fullPath).Length
        };

        _context.Documentos.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new DocumentoResponse
        {
            Id = entity.Id,
            CausaId = entity.CausaId,
            NumeroProceso = causa.NumeroProceso,
            Nombre = entity.Nombre,
            RutaArchivo = entity.RutaArchivo,
            TipoMime = entity.TipoMime,
            TamanoBytes = entity.TamanoBytes,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Documentos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entity.RutaArchivo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        _context.Documentos.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

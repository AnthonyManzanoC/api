using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Testimonios;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class TestimonioService : ITestimonioService
{
    private readonly AppDbContext _ctx;

    // ─── Diccionario de palabras prohibidas (español) ────────────
    private static readonly HashSet<string> _palabrasProhibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        // ofensas generales
        "idiota","imbécil","imbecil","estúpido","estupido","mierda","puta","puto",
        "maldito","maldita","inútil","inutil","basura","asco","vergüenza","verguenza",
        "hdp","hijodeputa","maricón","maricon","pendejo","pendeja","culero","culera",
        "cabron","cabrón","coño","cono","culo","chinga","chingada","joder",
        "carajo","coño","perra","perro","animal","bruto","bruta","mentiroso","mentirosa",
        "ladrón","ladron","fraude","estafador","estafadora","corrupto","corrupta",
        "odio","odiar","matar","muerte","asesino","asesina",
        // insultos raciales / discriminación (abreviados para no reproducirlos)
        "negro","negra","indio","india","mongol","retrasado","retrasada",
        // spam / irrelevante
        "spam","fake","falso","falsa","mentira","engaño","engano"
    };

    public TestimonioService(AppDbContext ctx) => _ctx = ctx;

    // ── Queries ───────────────────────────────────────────────────

    public async Task<List<TestimonioResponse>> GetAprobadosAsync(CancellationToken ct = default)
        => await _ctx.Testimonios
            .Where(x => x.Estado == EstadoTestimonio.Aprobado && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(Map())
            .ToListAsync(ct);

    public async Task<List<TestimonioResponse>> GetPendientesAsync(CancellationToken ct = default)
        => await _ctx.Testimonios
            .Where(x => x.Estado == EstadoTestimonio.Pendiente && x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .Select(Map())
            .ToListAsync(ct);

    public async Task<List<TestimonioResponse>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Testimonios
            .OrderByDescending(x => x.CreatedAt)
            .Select(Map())
            .ToListAsync(ct);

    // ── Crear ─────────────────────────────────────────────────────

    public async Task<TestimonioResponse> CreateAsync(
        TestimonioCreateRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Comentario))
            throw new InvalidOperationException("El comentario es obligatorio.");

        bool tieneHate = DetectarHate(request.Comentario);

        var entity = new Testimonio
        {
            Nombre = request.Nombre.Trim(),
            Comentario = request.Comentario.Trim(),
            Calificacion = Math.Clamp(request.Calificacion, 1, 5),
            FlagHate = tieneHate,
            // Si tiene hate → Pendiente (requiere revisión admin)
            // Si es limpio  → Aprobado automáticamente
            Estado = tieneHate
                           ? EstadoTestimonio.Pendiente
                           : EstadoTestimonio.Aprobado
        };

        _ctx.Testimonios.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return ToResponse(entity);
    }

    // ── Moderar ───────────────────────────────────────────────────

    public async Task<bool> ModerarAsync(
        Guid id, TestimonioUpdateEstadoRequest request, CancellationToken ct = default)
    {
        var entity = await _ctx.Testimonios.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.Estado = request.Estado;
        entity.NotaAdmin = request.NotaAdmin?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _ctx.Testimonios.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;
        _ctx.Testimonios.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    // ── Hate-speech filter ────────────────────────────────────────

    private static bool DetectarHate(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;

        // Normalizar: quitar acentos y minúsculas
        var normalizado = NormalizarTexto(texto);

        // Separar en palabras (letras, ignora puntuación)
        var palabras = System.Text.RegularExpressions.Regex
            .Matches(normalizado, @"\b\w+\b")
            .Select(m => m.Value);

        return palabras.Any(p => _palabrasProhibidas.Contains(p));
    }

    private static string NormalizarTexto(string texto)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in texto.Normalize(System.Text.NormalizationForm.FormD))
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLower(c));
        }
        return sb.ToString();
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static System.Linq.Expressions.Expression<Func<Testimonio, TestimonioResponse>> Map()
        => x => new TestimonioResponse
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Comentario = x.Comentario,
            Calificacion = x.Calificacion,
            Estado = x.Estado,
            FlagHate = x.FlagHate,
            NotaAdmin = x.NotaAdmin,
            CreatedAt = x.CreatedAt
        };

    private static TestimonioResponse ToResponse(Testimonio x) => new()
    {
        Id = x.Id,
        Nombre = x.Nombre,
        Comentario = x.Comentario,
        Calificacion = x.Calificacion,
        Estado = x.Estado,
        FlagHate = x.FlagHate,
        NotaAdmin = x.NotaAdmin,
        CreatedAt = x.CreatedAt
    };
}

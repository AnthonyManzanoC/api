using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Configuracion;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Services;

public class ConfiguracionEstudioService : IConfiguracionEstudioService
{
    private readonly AppDbContext _context;

    public ConfiguracionEstudioService(AppDbContext context) => _context = context;

    public async Task<ConfiguracionEstudioResponse> GetAsync(CancellationToken ct = default)
    {
        var cfg = await _context.ConfiguracionesEstudio
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (cfg is null)
        {
            cfg = new ConfiguracionEstudio
            {
                NombreEstudio = "Lamilla Escudero & Asociados",
                Slogan = "Solidez jurídica, trato humano y estrategia clara.",
                ColorPrimario = "#0B1F3A",
                ColorSecundario = "#D4AF37",
                ColorFondo = "#F7F8FA",
                IsActive = true
            };
            _context.ConfiguracionesEstudio.Add(cfg);
            await _context.SaveChangesAsync(ct);
        }

        return ToResponse(cfg);
    }

    public async Task<bool> UpdateAsync(
        Guid id, ConfiguracionEstudioUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _context.ConfiguracionesEstudio
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.NombreEstudio = req.NombreEstudio.Trim();
        entity.Slogan = Trim(req.Slogan);
        entity.LogoUrl = Trim(req.LogoUrl);
        entity.PhotoUrl = Trim(req.PhotoUrl);   // ✅ FASE 8
        entity.ColorPrimario = Trim(req.ColorPrimario);
        entity.ColorSecundario = Trim(req.ColorSecundario);
        entity.ColorFondo = Trim(req.ColorFondo);
        entity.EmailContacto = Trim(req.EmailContacto);
        entity.TelefonoContacto = Trim(req.TelefonoContacto);
        entity.Direccion = Trim(req.Direccion);
        entity.IsActive = req.IsActive;

        // SMTP
        entity.SmtpServer = Trim(req.SmtpServer);
        entity.SmtpPort = req.SmtpPort > 0 ? req.SmtpPort : 587;
        entity.SmtpUser = Trim(req.SmtpUser);
        entity.SmtpPass = Trim(req.SmtpPass);
        entity.SmtpEnableSsl = req.SmtpEnableSsl;
        entity.SmtpFromName = Trim(req.SmtpFromName);

        // Mapa
        entity.MapLatitude = req.MapLatitude;
        entity.MapLongitude = req.MapLongitude;
        entity.MapEmbedUrl = Trim(req.MapEmbedUrl);

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static ConfiguracionEstudioResponse ToResponse(ConfiguracionEstudio c) => new()
    {
        Id = c.Id,
        NombreEstudio = c.NombreEstudio,
        Slogan = c.Slogan,
        LogoUrl = c.LogoUrl,
        PhotoUrl = c.PhotoUrl,          // ✅ FASE 8
        ColorPrimario = c.ColorPrimario,
        ColorSecundario = c.ColorSecundario,
        ColorFondo = c.ColorFondo,
        EmailContacto = c.EmailContacto,
        TelefonoContacto = c.TelefonoContacto,
        Direccion = c.Direccion,
        IsActive = c.IsActive,
        SmtpServer = c.SmtpServer,
        SmtpPort = c.SmtpPort,
        SmtpUser = c.SmtpUser,
        SmtpPass = c.SmtpPass,
        SmtpEnableSsl = c.SmtpEnableSsl,
        SmtpFromName = c.SmtpFromName,
        MapLatitude = c.MapLatitude,
        MapLongitude = c.MapLongitude,
        MapEmbedUrl = c.MapEmbedUrl
    };

    private static string? Trim(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
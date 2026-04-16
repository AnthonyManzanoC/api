using LamillaEscudero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Cliente> Clientes { get; }
    DbSet<Causa> Causas { get; }
    DbSet<EventoProcesal> EventosProcesales { get; }
    DbSet<Plazo> Plazos { get; }
    DbSet<Documento> Documentos { get; }
    DbSet<ConsultaWeb> ConsultasWeb { get; }
    DbSet<ConfiguracionEstudio> ConfiguracionesEstudio { get; }
    DbSet<Notificacion> Notificaciones { get; }

    // ✅ NUEVOS
    DbSet<MiembroEstudio> MiembrosEstudio { get; }
    DbSet<ServicioOfrecido> ServiciosOfrecidos { get; }
      DbSet<Abono> Abonos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
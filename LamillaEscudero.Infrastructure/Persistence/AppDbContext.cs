using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Domain.Entities;
using LamillaEscudero.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LamillaEscudero.Infrastructure.Persistence;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
     public DbSet<Abono> Abonos => Set<Abono>();

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Causa> Causas => Set<Causa>();
    public DbSet<EventoProcesal> EventosProcesales => Set<EventoProcesal>();
    public DbSet<Plazo> Plazos => Set<Plazo>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<ConsultaWeb> ConsultasWeb => Set<ConsultaWeb>();
    public DbSet<ConfiguracionEstudio> ConfiguracionesEstudio => Set<ConfiguracionEstudio>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    // ✅ NUEVOS
    public DbSet<MiembroEstudio> MiembrosEstudio => Set<MiembroEstudio>();
    public DbSet<ServicioOfrecido> ServiciosOfrecidos => Set<ServicioOfrecido>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>(e =>
        {
            e.Property(x => x.Nombres).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cedula).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.UserId).HasMaxLength(450);

            // ✅ FASE 10
            e.Property(x => x.TotalHonorarios).HasColumnType("decimal(18,2)").HasDefaultValue(0m);

            e.HasIndex(x => x.Cedula);
            e.HasIndex(x => x.UserId);
        });
        builder.Entity<Abono>(e =>
        {
            e.Property(x => x.Monto).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(x => x.FechaAbono).IsRequired();
            e.Property(x => x.Observacion).HasMaxLength(300);

            e.HasOne(x => x.Cliente)
             .WithMany(x => x.Abonos)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Causa>(e =>
        {
            e.Property(x => x.NumeroProceso).HasMaxLength(50).IsRequired(false);
            e.Property(x => x.ExpedienteFiscal).HasMaxLength(80);
            e.Property(x => x.UnidadJudicial).HasMaxLength(250);
            e.Property(x => x.Materia).HasMaxLength(150);
            e.Property(x => x.Estado).HasMaxLength(100);
            e.HasIndex(x => x.NumeroProceso).IsUnique(false);
            e.HasIndex(x => x.ExpedienteFiscal).IsUnique(false);
            e.HasOne(x => x.Cliente)
             .WithMany(x => x.Causas)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EventoProcesal>(e =>
        {
            e.Property(x => x.Tipo).HasMaxLength(100).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Fuente).HasMaxLength(200);
            e.Property(x => x.RequiereAsistencia).HasDefaultValue(false);
            e.HasIndex(x => new { x.RequiereAsistencia, x.FechaHoraAgendada });
            e.HasOne(x => x.Causa)
             .WithMany(x => x.Eventos)
             .HasForeignKey(x => x.CausaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Plazo>(e =>
        {
            e.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
            e.Property(x => x.Notas).HasMaxLength(1000);
            e.HasOne(x => x.EventoProcesal)
             .WithMany(x => x.Plazos)
             .HasForeignKey(x => x.EventoProcesalId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Documento>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(250).IsRequired();
            e.Property(x => x.RutaArchivo).HasMaxLength(500).IsRequired();
            e.Property(x => x.TipoMime).HasMaxLength(100);
            e.HasOne(x => x.Causa)
             .WithMany(x => x.Documentos)
             .HasForeignKey(x => x.CausaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConsultaWeb>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.Asunto).HasMaxLength(200).IsRequired();
            e.Property(x => x.Mensaje).HasMaxLength(2000).IsRequired();
        });

        builder.Entity<Notificacion>(e =>
        {
            e.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
            e.Property(x => x.Mensaje).HasMaxLength(1500).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            e.Property(x => x.RelacionTipo).HasMaxLength(100);
            e.Property(x => x.FechaProgramada).IsRequired();
        });

        builder.Entity<ConfiguracionEstudio>(e =>
        {
            e.Property(x => x.NombreEstudio).HasMaxLength(250).IsRequired();
            e.Property(x => x.Slogan).HasMaxLength(250);
            e.Property(x => x.LogoUrl).HasMaxLength(500);
            e.Property(x => x.ColorPrimario).HasMaxLength(20);
            e.Property(x => x.ColorSecundario).HasMaxLength(20);
            e.Property(x => x.ColorFondo).HasMaxLength(20);
            e.Property(x => x.EmailContacto).HasMaxLength(200);
            e.Property(x => x.TelefonoContacto).HasMaxLength(30);
            e.Property(x => x.Direccion).HasMaxLength(300);

            // ✅ FASE 8 — Al no poner HasColumnType, EF Core usará "text" automáticamente en Postgres
            e.Property(x => x.PhotoUrl);
        });

        builder.Entity<MiembroEstudio>(e =>
        {
            e.Property(x => x.Nombres).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cargo).HasMaxLength(150).IsRequired();
            e.Property(x => x.BiografiaBreve).HasMaxLength(500);
            e.Property(x => x.FotoUrl).HasMaxLength(500);
            e.Property(x => x.LinkedIn).HasMaxLength(300);
            e.Property(x => x.Email).HasMaxLength(200);

            // ── FASE 9: columnas de texto largo (Corregido para Postgres) ──
            e.Property(x => x.PhotoData);
            e.Property(x => x.FraseDestacada);
            e.Property(x => x.BiografiaLarga);
            e.Property(x => x.EducacionJson);
            e.Property(x => x.TimelineExperienciaJson);
        });

        builder.Entity<ServicioOfrecido>(e =>
        {
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.DescripcionCorta).HasMaxLength(400).IsRequired();
            e.Property(x => x.Icono).HasMaxLength(50);
        });
    }
}
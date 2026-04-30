using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Infrastructure.Identity;
using LamillaEscudero.Infrastructure.Persistence;
using LamillaEscudero.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LamillaEscudero.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 👇 AQUÍ ESTÁ EL CAMBIO MÁGICO A POSTGRES 👇
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ── Servicios anteriores ─────────────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ICausaService, CausaService>();
        services.AddScoped<IEventoProcesalService, EventoProcesalService>();
        services.AddScoped<IPlazoService, PlazoService>();
        services.AddScoped<IDocumentoService, DocumentoService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IConfiguracionEstudioService, ConfiguracionEstudioService>();
        services.AddScoped<IConsultaWebService, ConsultaWebService>();
        services.AddScoped<INotificacionService, NotificacionService>();
        services.AddScoped<IAutomationService, AutomationService>();
        // ✅ FASE 10
        services.AddScoped<ITestimonioService, TestimonioService>();
        // ✅ LOS SERVICIOS NUEVOS
        services.AddScoped<IMiembroEstudioService, MiembroEstudioService>();
        services.AddScoped<IServicioOfrecidoService, ServicioOfrecidoService>();
        services.AddScoped<ICentroCuentasService, CentroCuentasService>();
        services.AddScoped<IPublicChatService, PublicChatService>();

        // Si ya creaste el archivo EmailService de la Fase 7, descomenta la siguiente línea:
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}

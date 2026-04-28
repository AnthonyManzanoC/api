using LamillaEscudero.Infrastructure;
using Microsoft.AspNetCore.Identity;
using LamillaEscudero.Infrastructure.Identity;
using Hangfire;
using Hangfire.PostgreSql; // <--- ¡AGREGA ESTA LÍNEA AQUÍ TAMBIÉN!
var builder = WebApplication.CreateBuilder(args);

// ── Razor + Blazor ──────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Infraestructura (DbContext + Identity + Servicios) ──────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Cookie de Identity ──────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/acceso-denegado";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy =
        Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite =
        Microsoft.AspNetCore.Http.SameSiteMode.Lax;
});

// ── Antiforgery ─────────────────────────────────────────────
builder.Services.AddAntiforgery();

// ── Auth state cascading para Blazor ────────────────────────
builder.Services.AddCascadingAuthenticationState();

// ── HTTP Client para página pública ─────────────────────────
builder.Services.AddHttpClient<LamillaEscudero.Web.Services.PublicApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5151/");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});
// Antes decía: config.UseSqlServerStorage(...)
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ ORDEN SAGRADO — no alterar:
// UseAntiforgery() ANTES de UseAuthentication()
// MapRazorComponents() gestiona el routing, NO añadir UseRouting() explícito
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

// Esto le dice al robot: "Ejecuta las alertas todos los días a la medianoche"
RecurringJob.AddOrUpdate<LamillaEscudero.Application.Abstractions.IAutomationService>(
    "generar-alertas-diarias",
    servicio => servicio.EjecutarAsync(default),
    Cron.Daily);
// ── Endpoint de logout (POST limpio sin Blazor) ──────────────
app.MapPost("/logout", async (SignInManager<ApplicationUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.Redirect("/login");
})
.RequireAuthorization()
.WithName("Logout");

// ── Razor Components ─────────────────────────────────────────
app.MapRazorComponents<LamillaEscudero.Web.Components.App>()
    .AddInteractiveServerRenderMode();
app.MapGet("/ping", () => Results.Ok("Despierto"));
app.Run();
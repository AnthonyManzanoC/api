using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using LamillaEscudero.Infrastructure;
using LamillaEscudero.Infrastructure.Seed;
using LamillaEscudero.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString no configurada.");

// 👇 CONFIGURACIÓN DE HANGFIRE PARA POSTGRES 👇
builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UsePostgreSqlStorage(connectionString);
});
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada.");
var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
var jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ==========================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ==========================================
WebApplication app;
try
{
    app = builder.Build();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    var erroresReales = string.Join("\n", ex.LoaderExceptions.Select(e => e?.Message));
    throw new Exception($"\n=== ERROR REAL DETECTADO ===\n{erroresReales}\n===========================\n", ex);
}

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 👇 DASHBOARD DE HANGFIRE (Afuera del IsDevelopment para que puedas verlo en Render)
app.UseHangfireDashboard("/hangfire");

await DbSeeder.SeedAsync(app.Services);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 👇 SOLUCIÓN AL ERROR DE RENDER 👇
// Inyectamos el servicio correctamente en lugar de usar la clase estática
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IAutomationService>(
        "recordatorios-diarios",
        x => x.EjecutarAsync(CancellationToken.None),
        Cron.Daily() // Importante: usar los paréntesis ()
    );
}

app.Run();
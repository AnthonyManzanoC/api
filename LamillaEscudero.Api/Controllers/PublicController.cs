using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Consultas;
using LamillaEscudero.Application.Models.Testimonios;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IConfiguracionEstudioService _configService;
    private readonly IConsultaWebService _consultaService;
    private readonly ITestimonioService _testimonioService;

    public PublicController(
        IConfiguracionEstudioService configService,
        IConsultaWebService consultaService,
        ITestimonioService testimonioService)
    {
        _configService = configService;
        _consultaService = consultaService;
        _testimonioService = testimonioService;
    }

    [HttpGet("configuracion")]
    public async Task<IActionResult> GetConfiguracion(CancellationToken ct)
    {
        var config = await _configService.GetAsync(ct);

        return Ok(new
        {
            config.Id,
            config.NombreEstudio,
            config.Slogan,
            config.LogoUrl,
            config.ColorPrimario,
            config.ColorSecundario,
            config.ColorFondo,
            config.EmailContacto,
            config.TelefonoContacto,
            config.Direccion,
            config.MapLatitude,
            config.MapLongitude,
            config.MapEmbedUrl
        });
    }

    [HttpPost("consultas")]
    public async Task<IActionResult> CrearConsulta(
        [FromBody] ConsultaWebCreateRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _consultaService.CreateAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("testimonios")]
    public async Task<IActionResult> GetTestimonios(CancellationToken ct)
        => Ok(await _testimonioService.GetAprobadosAsync(ct));

    [HttpPost("testimonios")]
    public async Task<IActionResult> CrearTestimonio(
        [FromBody] TestimonioCreateRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _testimonioService.CreateAsync(request, ct);
            return Ok(new
            {
                created.Id,
                created.Estado,
                mensaje = created.Estado == "Aprobado"
                    ? "¡Gracias! Su testimonio ya es visible."
                    : "¡Gracias! Su testimonio está en revisión y será publicado pronto."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

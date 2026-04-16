using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Consultas;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IConfiguracionEstudioService _configService;
    private readonly IConsultaWebService _consultaService;

    public PublicController(
        IConfiguracionEstudioService configService,
        IConsultaWebService consultaService)
    {
        _configService = configService;
        _consultaService = consultaService;
    }

    [HttpGet("configuracion")]
    public async Task<IActionResult> GetConfiguracion(CancellationToken cancellationToken)
        => Ok(await _configService.GetAsync(cancellationToken));

    [HttpPost("consultas")]
    public async Task<IActionResult> CrearConsulta([FromBody] ConsultaWebCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _consultaService.CreateAsync(request, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
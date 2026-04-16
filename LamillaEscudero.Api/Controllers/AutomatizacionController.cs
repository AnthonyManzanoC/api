using LamillaEscudero.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Abogado")]
public class AutomatizacionController : ControllerBase
{
    private readonly IAutomationService _service;

    public AutomatizacionController(IAutomationService service)
    {
        _service = service;
    }

    [HttpPost("ejecutar")]
    public async Task<IActionResult> Ejecutar(CancellationToken cancellationToken)
    {
        await _service.EjecutarAsync(cancellationToken);
        return Ok(new { message = "Automatización ejecutada correctamente." });
    }
}
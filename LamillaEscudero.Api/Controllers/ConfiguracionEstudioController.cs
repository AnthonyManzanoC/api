using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Configuracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Abogado")]
public class ConfiguracionEstudioController : ControllerBase
{
    private readonly IConfiguracionEstudioService _service;

    public ConfiguracionEstudioController(IConfiguracionEstudioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _service.GetAsync(cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ConfiguracionEstudioUpdateRequest request, CancellationToken cancellationToken)
    {
        var ok = await _service.UpdateAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Plazos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlazosController : ControllerBase
{
    private readonly IPlazoService _service;

    public PlazosController(IPlazoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("proximos")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7, CancellationToken cancellationToken = default)
        => Ok(await _service.GetUpcomingAsync(days, cancellationToken));

    [HttpGet("evento/{eventoProcesalId:guid}")]
    public async Task<IActionResult> GetByEvento(Guid eventoProcesalId, CancellationToken cancellationToken)
        => Ok(await _service.GetByEventoProcesalIdAsync(eventoProcesalId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetByIdAsync(id, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlazoCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PlazoUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _service.UpdateAsync(id, request, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/cumplido")]
    public async Task<IActionResult> MarkCumplido(Guid id, [FromBody] PlazoCumplimientoRequest request, CancellationToken cancellationToken)
    {
        var ok = await _service.MarkCumplidoAsync(id, request.Cumplido, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _service.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Eventos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventosProcesalesController : ControllerBase
{
    private readonly IEventoProcesalService _service;

    public EventosProcesalesController(IEventoProcesalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("causa/{causaId:guid}")]
    public async Task<IActionResult> GetByCausaId(Guid causaId, CancellationToken cancellationToken)
        => Ok(await _service.GetByCausaIdAsync(causaId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetByIdAsync(id, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventoProcesalCreateRequest request, CancellationToken cancellationToken)
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
    public async Task<IActionResult> Update(Guid id, [FromBody] EventoProcesalUpdateRequest request, CancellationToken cancellationToken)
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _service.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
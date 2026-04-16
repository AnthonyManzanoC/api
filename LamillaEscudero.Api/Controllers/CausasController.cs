using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Causas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todo el controlador
public class CausasController : ControllerBase
{
    private readonly ICausaService _causaService;

    public CausasController(ICausaService causaService)
    {
        _causaService = causaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var data = await _causaService.GetAllAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("cliente/{clienteId:guid}")]
    public async Task<IActionResult> GetByClienteId(Guid clienteId, CancellationToken cancellationToken)
    {
        var data = await _causaService.GetByClienteIdAsync(clienteId, cancellationToken);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _causaService.GetByIdAsync(id, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CausaCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _causaService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CausaUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _causaService.UpdateAsync(id, request, cancellationToken);
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
        var ok = await _causaService.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
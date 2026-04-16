using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todo el controlador
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var data = await _clienteService.GetAllAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _clienteService.GetByIdAsync(id, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClienteCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _clienteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClienteUpdateRequest request, CancellationToken cancellationToken)
    {
        var ok = await _clienteService.UpdateAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _clienteService.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
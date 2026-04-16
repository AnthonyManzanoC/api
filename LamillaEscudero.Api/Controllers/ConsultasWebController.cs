using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Consultas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Abogado,Asistente")]
public class ConsultasWebController : ControllerBase
{
    private readonly IConsultaWebService _service;

    public ConsultasWebController(IConsultaWebService service)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("no-leidas")]
    public async Task<IActionResult> GetUnRead(CancellationToken ct)
        => Ok(await _service.GetUnReadAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var data = await _service.GetByIdAsync(id, ct);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPatch("{id:guid}/leida")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var ok = await _service.MarkAsReadAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Actualiza estado CRM: estado, fecha de cita, notas internas</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] ConsultaWebUpdateRequest request, CancellationToken ct)
    {
        var ok = await _service.UpdateAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}

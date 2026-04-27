using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Testimonios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Abogado")]
public class TestimoniosController : ControllerBase
{
    private readonly ITestimonioService _service;
    public TestimoniosController(ITestimonioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("pendientes")]
    public async Task<IActionResult> GetPendientes(CancellationToken ct)
        => Ok(await _service.GetPendientesAsync(ct));

    [HttpPatch("{id:guid}/moderar")]
    public async Task<IActionResult> Moderar(
        Guid id, [FromBody] TestimonioUpdateEstadoRequest request, CancellationToken ct)
    {
        var ok = await _service.ModerarAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
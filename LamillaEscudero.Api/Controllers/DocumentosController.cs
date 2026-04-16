using LamillaEscudero.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LamillaEscudero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _service;

    public DocumentosController(IDocumentoService service)
    {
        _service = service;
    }

    [HttpGet("causa/{causaId:guid}")]
    public async Task<IActionResult> GetByCausaId(Guid causaId, CancellationToken cancellationToken)
        => Ok(await _service.GetByCausaIdAsync(causaId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetByIdAsync(id, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost("subir")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid causaId,
        [FromForm] string nombre,
        [FromForm] IFormFile archivo,
        CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { message = "Debes enviar un archivo válido." });

        try
        {
            await using var stream = archivo.OpenReadStream();

            var created = await _service.UploadAsync(
                causaId,
                nombre,
                stream,
                archivo.FileName,
                archivo.ContentType,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
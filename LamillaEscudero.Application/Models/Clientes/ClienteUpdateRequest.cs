namespace LamillaEscudero.Application.Models.Clientes;

public class ClienteUpdateRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Observaciones { get; set; }
    public bool IsActive { get; set; } = true;
}
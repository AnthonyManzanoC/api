namespace LamillaEscudero.Application.Models.Consultas;

public class ConsultaWebCreateRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

namespace LamillaEscudero.Web.Models;

public class ConsultaPublicaResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime CreatedAt { get; set; }
}
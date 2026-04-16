namespace LamillaEscudero.Application.Models.Consultas;

public class ConsultaWebResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public DateTime? FechaCita { get; set; }
    public string? NotasInternas { get; set; }
    public DateTime CreatedAt { get; set; }
}


/// <summary>Permite al admin actualizar estado, cita y notas internas</summary>
public class ConsultaWebUpdateRequest
{
    public string Estado { get; set; } = "Pendiente";
    public DateTime? FechaCita { get; set; }
    public string? NotasInternas { get; set; }
}

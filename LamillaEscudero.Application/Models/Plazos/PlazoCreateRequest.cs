namespace LamillaEscudero.Application.Models.Plazos;

public class PlazoCreateRequest
{
    public Guid EventoProcesalId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public bool ConfirmadoPorAbogado { get; set; }
    public bool Cumplido { get; set; }
    public string? Notas { get; set; }
}
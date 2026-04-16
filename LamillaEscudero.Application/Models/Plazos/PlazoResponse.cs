namespace LamillaEscudero.Application.Models.Plazos;

public class PlazoResponse
{
    public Guid Id { get; set; }
    public Guid EventoProcesalId { get; set; }
    public Guid CausaId { get; set; }
    public string NumeroProceso { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public bool ConfirmadoPorAbogado { get; set; }
    public bool Cumplido { get; set; }
    public string? Notas { get; set; }
    public string? TipoEvento { get; set; }
    public DateTime? FechaEvento { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
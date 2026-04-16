namespace LamillaEscudero.Application.Models.Causas;

public class CausaUpdateRequest
{
    public Guid ClienteId { get; set; }
    public string? NumeroProceso { get; set; }
    public string? ExpedienteFiscal { get; set; }
    public string? UnidadJudicial { get; set; }
    public string? Materia { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public string? Resumen { get; set; }
    public bool IsActive { get; set; } = true;
}

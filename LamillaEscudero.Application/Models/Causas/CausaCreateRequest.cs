namespace LamillaEscudero.Application.Models.Causas;

public class CausaCreateRequest
{
    public Guid ClienteId { get; set; }

    // Nullable: las causas penales en fase de investigación aún no tienen proceso judicial
    public string? NumeroProceso { get; set; }

    // Número de expediente fiscal (Fiscalía / investigación previa)
    public string? ExpedienteFiscal { get; set; }

    public string? UnidadJudicial { get; set; }
    public string? Materia { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public string? Resumen { get; set; }
}

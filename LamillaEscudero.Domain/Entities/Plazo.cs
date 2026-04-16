using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class Plazo : BaseEntity
{
    public Guid EventoProcesalId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public bool ConfirmadoPorAbogado { get; set; }
    public bool Cumplido { get; set; }
    public string? Notas { get; set; }

    public EventoProcesal? EventoProcesal { get; set; }
}
using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class Causa : BaseEntity
{
    public Guid ClienteId { get; set; }

    // ✅ NULLABLE: causas penales pueden iniciar sin proceso judicial
    public string? NumeroProceso { get; set; }

    // ✅ NUEVO: número de expediente en Fiscalía (fase investigación previa)
    public string? ExpedienteFiscal { get; set; }

    public string? UnidadJudicial { get; set; }
    public string? Materia { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public string? Resumen { get; set; }

    public Cliente? Cliente { get; set; }
    public List<EventoProcesal> Eventos { get; set; } = new();
    public List<Documento> Documentos { get; set; } = new();
}

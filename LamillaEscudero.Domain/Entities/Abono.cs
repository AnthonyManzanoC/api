using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

/// <summary>
/// Registro de un pago / abono realizado por un cliente
/// contra el total de honorarios pactados.
/// </summary>
public class Abono : BaseEntity
{
    public Guid ClienteId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaAbono { get; set; } = DateTime.UtcNow;
    public string? Observacion { get; set; }

    // Navegación
    public Cliente? Cliente { get; set; }
}
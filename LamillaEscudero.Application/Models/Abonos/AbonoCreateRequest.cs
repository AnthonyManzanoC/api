namespace LamillaEscudero.Application.Models.Abonos;

// ── Request ───────────────────────────────────────────────────────
public class AbonoCreateRequest
{
    public Guid ClienteId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaAbono { get; set; } = DateTime.UtcNow;
    public string? Observacion { get; set; }
}

namespace LamillaEscudero.Application.Models.Abonos;
// ── Response ──────────────────────────────────────────────────────
public class AbonoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaAbono { get; set; }
    public string? Observacion { get; set; }
    public DateTime CreatedAt { get; set; }
}

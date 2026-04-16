using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class Cliente : BaseEntity
{
    public string Nombres { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Observaciones { get; set; }

    // ✅ Vínculo con Identity (portal del cliente)
    public string? UserId { get; set; }

    // ✅ FASE 10: Honorarios pactados
    public decimal TotalHonorarios { get; set; } = 0;

    // Navegación
    public List<Causa> Causas { get; set; } = new();
    public List<Abono> Abonos { get; set; } = new();
}
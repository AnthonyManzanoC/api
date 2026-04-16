namespace LamillaEscudero.Application.Models.Clientes;

public class ClienteResponse
{
    public Guid Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Observaciones { get; set; }
    public string? UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // ✅ FASE 10 — Honorarios
    public decimal TotalHonorarios { get; set; }
    public decimal TotalAbonado { get; set; }   // suma de abonos (calculado en servicio)
    public decimal SaldoPendiente => TotalHonorarios - TotalAbonado;
}
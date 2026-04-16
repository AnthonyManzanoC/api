using LamillaEscudero.Domain.Common;

namespace LamillaEscudero.Domain.Entities;

public class ServicioOfrecido : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string DescripcionCorta { get; set; } = string.Empty;
    public string? Detalles { get; set; }
    public string? Icono { get; set; } = "bi-briefcase";
    public int Orden { get; set; } = 0;
}
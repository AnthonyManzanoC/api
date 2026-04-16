

namespace LamillaEscudero.Application.Models.Servicios
{

    public class ServicioOfrecidoResponse
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string DescripcionCorta { get; set; } = string.Empty;
        public string? Detalles { get; set; }
        public string? Icono { get; set; }
        public int Orden { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

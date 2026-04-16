

namespace LamillaEscudero.Application.Models.Servicios
{
    public class ServicioOfrecidoCreateRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string DescripcionCorta { get; set; } = string.Empty;
        public string? Detalles { get; set; }
        public string? Icono { get; set; } = "bi-briefcase";
        public int Orden { get; set; } = 0;
    }
}



namespace LamillaEscudero.Application.Models.Servicios
{
    public class ServicioOfrecidoUpdateRequest : ServicioOfrecidoCreateRequest
    {
        public bool IsActive { get; set; } = true;
    }

}
